using System.Text.Json;
using Bridge.Api.Contracts;
using Bridge.Api.Data;
using Bridge.Api.Domain;
using Bridge.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Api;

public static class BridgeEndpoints
{
    public static IEndpointRouteBuilder MapBridgeEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireRateLimiting("api");

        api.MapPost("/users/anonymous", async (CreateAnonymousUserRequest request, BridgeDbContext db, CancellationToken ct) =>
        {
            var user = new AppUser
            {
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? "Learner" : request.DisplayName.Trim()
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/users/{user.Id}", new { user.Id, user.DisplayName });
        });

        api.MapGet("/assessments/{id}", async (string id, BridgeDbContext db, CancellationToken ct) =>
        {
            var assessment = await db.AssessmentDefinitions.AsNoTracking().AsSplitQuery()
                .Where(x => x.Id == id && x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Version,
                    sections = x.Sections.OrderBy(section => section.Order).Select(section => new
                    {
                        section.Id,
                        section.Title,
                        section.Description,
                        section.Order,
                        questions = section.Questions.OrderBy(question => question.Order).Select(question => new
                        {
                            question.Id,
                            question.Code,
                            question.Prompt,
                            question.AnswerType,
                            question.Order,
                            question.MinSelections,
                            question.MaxSelections,
                            question.ScaleMin,
                            question.ScaleMax,
                            question.ScaleMinLabel,
                            question.ScaleMaxLabel,
                            question.IsRequired,
                            options = question.Options.OrderBy(option => option.Order).Select(option => new
                            {
                                option.Id,
                                option.Value,
                                option.Label,
                                option.Order
                            })
                        })
                    })
                })
                .SingleOrDefaultAsync(ct);

            return assessment is null ? Results.NotFound() : Results.Ok(assessment);
        });

        // Submit a completed persona assessment: validate, persist the attempt and
        // every answer, then (once a system prompt is configured) run AI analysis.
        api.MapPost("/assessments/{id}/submit", async (
            string id,
            SubmitPersonaAssessmentRequest request,
            BridgeDbContext db,
            PersonaAssessmentService persona,
            PersonaPromptProvider promptProvider,
            IAiClient ai,
            Microsoft.Extensions.Options.IOptions<OpenAiOptions> aiOptions,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var user = await db.Users.Include(x => x.Profile).SingleOrDefaultAsync(x => x.Id == request.UserId, ct);
            if (user is null) return Results.NotFound(new { error = "User not found." });

            var definition = await db.AssessmentDefinitions.AsNoTracking().AsSplitQuery()
                .Include(x => x.Sections).ThenInclude(section => section.Questions).ThenInclude(question => question.Options)
                .SingleOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
            if (definition is null) return Results.NotFound(new { error = "Assessment not found." });

            IReadOnlyList<PersonaNormalizedAnswer> normalized;
            try
            {
                normalized = persona.Validate(definition, request.Answers);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }

            var attempt = new AssessmentAttempt
            {
                UserId = user.Id,
                AssessmentId = definition.Id,
                AssessmentVersion = definition.Version
            };
            foreach (var answer in normalized)
            {
                attempt.Responses.Add(new AssessmentResponse
                {
                    AttemptId = attempt.Id,
                    QuestionId = answer.Question.Id,
                    QuestionCode = answer.Question.Code,
                    AnswerType = answer.Question.AnswerType,
                    SelectedValuesJson = answer.ScaleValue.HasValue ? null : JsonSerializer.Serialize(answer.SelectedValues),
                    ScaleValue = answer.ScaleValue
                });
            }

            // AI persona analysis is optional: it runs only when a system prompt has
            // been provided and an API key is configured. Otherwise the attempt is
            // saved as "pending" so the prompt can be plugged in and re-run later.
            PersonaAnalysis? analysis = null;
            var systemPrompt = promptProvider.GetSystemPrompt();
            if (!string.IsNullOrWhiteSpace(systemPrompt) && !string.IsNullOrWhiteSpace(aiOptions.Value.ApiKey))
            {
                try
                {
                    var input = persona.BuildAnalysisInput(definition, normalized);
                    var raw = await ai.GenerateAsync(systemPrompt, input, ct);
                    var json = ExtractJson(raw);
                    analysis = new PersonaAnalysis
                    {
                        AttemptId = attempt.Id,
                        UserId = user.Id,
                        PromptVersion = promptProvider.PromptVersion,
                        ModelName = aiOptions.Value.Model,
                        AnalysisJson = json,
                        Summary = TryReadSummary(json)
                    };
                    attempt.Analysis = analysis;
                    attempt.AnalysisStatus = "analyzed";
                }
                catch (Exception exception)
                {
                    loggerFactory.CreateLogger("PersonaAssessment")
                        .LogError(exception, "Persona analysis failed for user {UserId}", user.Id);
                    attempt.AnalysisStatus = "failed";
                }
            }

            // Keep the rest of the app working: upsert the user's profile from the
            // assessment answers, enriched by the AI analysis when it is available.
            int ScaleOf(string code) => normalized.FirstOrDefault(a => a.Question.Code == code)?.ScaleValue ?? 3;
            string? ChoiceOf(string code) => normalized.FirstOrDefault(a => a.Question.Code == code)?.SelectedValues.FirstOrDefault();

            var baseline = Math.Clamp(TryReadInt(analysis?.AnalysisJson, "confidence_baseline") ?? ScaleOf("Q2"), 1, 5);
            var recommendedActor =
                MapRecommendedActor(TryReadNestedString(analysis?.AnalysisJson, "first_learning_path", "recommended_actor"))
                ?? MapGoalToActor(ChoiceOf("Q11"))
                ?? "friend";

            user.Profile ??= new UserProfile { UserId = user.Id };
            user.Profile.BaselineConfidence = baseline;
            user.Profile.CurrentConfidence = baseline;
            user.Profile.HardestActorType = recommendedActor;
            user.Profile.ClassroomComfort = ScaleOf("Q3");
            user.Profile.DisagreementComfort = ScaleOf("Q4");
            user.Profile.SmallTalkComfort = ScaleOf("Q2");
            user.Profile.UsExperience = ChoiceOf("Q1") ?? user.Profile.UsExperience;
            user.Profile.AssessmentVersion = definition.Version;
            user.Profile.UpdatedAt = DateTimeOffset.UtcNow;

            db.AssessmentAttempts.Add(attempt);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                attemptId = attempt.Id,
                analysisStatus = attempt.AnalysisStatus,
                responseCount = attempt.Responses.Count,
                analysis = analysis is null ? null : new
                {
                    analysis.PromptVersion,
                    analysis.ModelName,
                    analysis.Summary,
                    persona = ParsePersona(analysis.AnalysisJson)
                }
            });
        });

        // Latest persona analysis for a user (for personalising later sessions).
        api.MapGet("/users/{id:guid}/persona", async (Guid id, BridgeDbContext db, CancellationToken ct) =>
        {
            var analysis = await db.PersonaAnalyses.AsNoTracking()
                .Where(x => x.UserId == id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
            return analysis is null
                ? Results.NotFound()
                : Results.Ok(new
                {
                    analysis.AttemptId,
                    analysis.PromptVersion,
                    analysis.ModelName,
                    analysis.Summary,
                    persona = ParsePersona(analysis.AnalysisJson),
                    analysis.CreatedAt
                });
        });

        api.MapGet("/users/{id:guid}", async (Guid id, BridgeDbContext db, CancellationToken ct) =>
        {
            var user = await db.Users.AsNoTracking().Include(x => x.Profile)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (user is null) return Results.NotFound();
            return Results.Ok(new
            {
                user.Id,
                user.DisplayName,
                profile = user.Profile is null ? null : new
                {
                    user.Profile.CountryOfOrigin,
                    user.Profile.UsExperience,
                    user.Profile.BaselineConfidence,
                    user.Profile.CurrentConfidence,
                    user.Profile.HardestActorType,
                    user.Profile.ClassroomComfort,
                    user.Profile.DisagreementComfort,
                    user.Profile.SmallTalkComfort
                }
            });
        });

        api.MapPost("/onboarding/assess", async (SaveOnboardingRequest request, BridgeDbContext db, OnboardingAssessmentService service, CancellationToken ct) =>
        {
            var user = await db.Users.Include(x => x.Profile)
                .SingleOrDefaultAsync(x => x.Id == request.UserId, ct);
            if (user is null) return Results.NotFound();

            OnboardingAssessmentResult result;
            try
            {
                result = service.Assess(request.Assessment);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }

            user.DisplayName = request.Assessment.DisplayName.Trim();
            user.Profile ??= new UserProfile { UserId = user.Id };
            var profile = user.Profile;
            profile.CountryOfOrigin = request.Assessment.CountryOfOrigin;
            profile.UsExperience = request.Assessment.UsExperience;
            profile.BaselineConfidence = result.BaselineConfidence;
            profile.CurrentConfidence = result.BaselineConfidence;
            profile.HardestActorType = result.RecommendedActorType;
            profile.ClassroomComfort = request.Assessment.ClassroomComfort;
            profile.DisagreementComfort = request.Assessment.DisagreementComfort;
            profile.SmallTalkComfort = request.Assessment.SmallTalkComfort;
            profile.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(result);
        });

        api.MapGet("/actors", async (BridgeDbContext db, CancellationToken ct) => Results.Ok(
            await db.Actors.AsNoTracking().Where(x => x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.ActorType,
                    x.Name,
                    x.Role,
                    x.Personality,
                    x.CommunicationStyle
                }).ToListAsync(ct)));

        api.MapGet("/scenarios", async (string? actor, BridgeDbContext db, CancellationToken ct) => Results.Ok(
            await db.Scenarios.AsNoTracking()
                .Where(x => x.IsActive && (actor == null || x.Actor.ActorType == actor))
                .Select(x => new
                {
                    x.Id,
                    x.ActorId,
                    x.Actor.ActorType,
                    x.Title,
                    x.Description,
                    x.Goal,
                    x.Difficulty,
                    x.OpeningMessage,
                    x.CultureContext
                }).ToListAsync(ct)));

        api.MapPost("/sessions", async (CreateSessionRequest request, BridgeDbContext db, CancellationToken ct) =>
        {
            if (request.DirectnessLevel is < 1 or > 5)
                return Results.BadRequest(new { error = "DirectnessLevel must be 1-5." });

            var user = await db.Users.Include(x => x.Profile).SingleOrDefaultAsync(x => x.Id == request.UserId, ct);
            var actor = await db.Actors.FindAsync([request.ActorId], ct);
            var scenario = await db.Scenarios.FindAsync([request.ScenarioId], ct);
            if (user?.Profile is null || actor is null || scenario is null || scenario.ActorId != actor.Id)
                return Results.BadRequest(new { error = "Invalid user profile, actor or scenario." });

            var session = new PracticeSession
            {
                UserId = user.Id,
                ActorId = actor.Id,
                ScenarioId = scenario.Id,
                DirectnessLevel = request.DirectnessLevel
            };
            session.Messages.Add(new ChatMessage
            {
                Role = "actor",
                Content = scenario.OpeningMessage,
                SequenceNumber = 1
            });
            db.PracticeSessions.Add(session);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/sessions/{session.Id}", new
            {
                session.Id,
                session.Status,
                actor = new { actor.Id, actor.Name, actor.Role },
                scenario = new { scenario.Id, scenario.Title, scenario.Goal },
                scenario.OpeningMessage
            });
        });

        api.MapGet("/sessions/{id:guid}", async (Guid id, BridgeDbContext db, CancellationToken ct) =>
        {
            var session = await db.PracticeSessions.AsNoTracking()
                .Include(x => x.Actor)
                .Include(x => x.Scenario)
                .Include(x => x.Messages)
                .Include(x => x.Evaluation)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            return session is null
                ? Results.NotFound()
                : Results.Ok(new
                {
                    session.Id,
                    session.UserId,
                    session.Status,
                    session.DirectnessLevel,
                    actor = new { session.Actor.Id, session.Actor.Name, session.Actor.Role },
                    scenario = new { session.Scenario.Id, session.Scenario.Title, session.Scenario.Goal },
                    messages = session.Messages.OrderBy(x => x.SequenceNumber).Select(x => new
                    {
                        x.Id,
                        x.Role,
                        x.Content,
                        x.GoalProgress,
                        x.CultureNoteType,
                        x.CultureNote,
                        x.CreatedAt
                    }),
                    evaluation = session.Evaluation is null ? null : ToEvaluation(session.Evaluation)
                });
        });

        api.MapPost("/sessions/{id:guid}/messages", async (Guid id, SendMessageRequest request, BridgeDbContext db, PromptBuilder prompts, IAiClient ai, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 2000)
                return Results.BadRequest(new { error = "Message must be 1-2000 characters." });

            var session = await db.PracticeSessions
                .Include(x => x.Actor)
                .Include(x => x.Scenario)
                .Include(x => x.User).ThenInclude(x => x.Profile)
                .Include(x => x.Messages)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session is null) return Results.NotFound();
            if (session.Status != "active") return Results.Conflict(new { error = "Session is not active." });

            var existing = session.Messages.FirstOrDefault(x => x.ClientMessageId == request.ClientMessageId);
            if (existing is not null) return Results.Ok(new { duplicate = true, existing.Id });

            var next = session.Messages.Count == 0 ? 1 : session.Messages.Max(x => x.SequenceNumber) + 1;
            var userMessage = new ChatMessage
            {
                SessionId = session.Id,
                ClientMessageId = request.ClientMessageId,
                Role = "user",
                Content = request.Content.Trim(),
                SequenceNumber = next
            };
            db.Messages.Add(userMessage);
            await db.SaveChangesAsync(ct);

            var transcript = string.Join("\n", session.Messages.OrderBy(x => x.SequenceNumber)
                .Select(x => $"{x.Role}: {x.Content}"));
            var raw = await ai.GenerateAsync(
                prompts.BuildRolePlayPrompt(session.Actor, session.Scenario, session.User.Profile!, session.DirectnessLevel),
                transcript,
                ct);
            var result = AiJson.Parse<ChatAiResult>(raw);
            var actorMessage = new ChatMessage
            {
                SessionId = session.Id,
                Role = "actor",
                Content = result.Reply,
                SequenceNumber = next + 1,
                GoalProgress = Math.Clamp(result.GoalProgress, 0, 100),
                CultureNoteType = result.CultureNoteType,
                CultureNote = result.CultureNote
            };
            db.Messages.Add(actorMessage);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                messageId = actorMessage.Id,
                result.Reply,
                result.CultureNoteType,
                result.CultureNote,
                goalProgress = actorMessage.GoalProgress
            });
        });

        api.MapPost("/sessions/{id:guid}/hint", async (Guid id, BridgeDbContext db, IAiClient ai, CancellationToken ct) =>
        {
            var session = await db.PracticeSessions.Include(x => x.Scenario).Include(x => x.Messages)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session is null) return Results.NotFound();
            var last = session.Messages.OrderBy(x => x.SequenceNumber).LastOrDefault()?.Content ?? "";
            var hint = await ai.GenerateAsync(
                "Give one short intent hint or sentence starter. Do not write the full answer.",
                $"Goal: {session.Scenario.Goal}\nLast turn: {last}",
                ct);
            return Results.Ok(new { hint });
        });

        api.MapPost("/sessions/{id:guid}/complete", async (Guid id, CompleteSessionRequest request, BridgeDbContext db, PromptBuilder prompts, IAiClient ai, Microsoft.Extensions.Options.IOptions<OpenAiOptions> options, CancellationToken ct) =>
        {
            if (!request.Confirmed) return Results.BadRequest(new { error = "Completion must be confirmed." });
            var session = await db.PracticeSessions
                .Include(x => x.Actor)
                .Include(x => x.Scenario)
                .Include(x => x.User).ThenInclude(x => x.Profile)
                .Include(x => x.Messages)
                .Include(x => x.Evaluation)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session is null) return Results.NotFound();
            if (session.Evaluation is not null) return Results.Ok(ToEvaluation(session.Evaluation));
            if (!session.Messages.Any(x => x.Role == "user"))
                return Results.BadRequest(new { error = "Send at least one message before completing." });

            var raw = await ai.GenerateAsync(
                prompts.BuildEvaluationPrompt(session.Actor, session.Scenario, session.User.Profile!, session.Messages),
                "Evaluate the transcript now.",
                ct);
            var result = AiJson.Parse<EvaluationAiResult>(raw);
            var evaluation = new SessionEvaluation
            {
                SessionId = session.Id,
                OverallScore = Clamp(result.OverallScore),
                ClarityScore = Clamp(result.Clarity),
                DirectnessScore = Clamp(result.Directness),
                WarmthScore = Clamp(result.Warmth),
                EngagementScore = Clamp(result.Engagement),
                GoalCompletionScore = Clamp(result.GoalCompletion),
                StrengthsJson = JsonSerializer.Serialize(result.Strengths),
                ImprovementsJson = JsonSerializer.Serialize(result.Improvements),
                CultureGapJson = JsonSerializer.Serialize(result.CultureGap),
                Summary = result.Summary,
                ModelName = options.Value.Model
            };
            db.SessionEvaluations.Add(evaluation);
            session.Evaluation = evaluation;
            session.Status = "completed";
            session.CompletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToEvaluation(evaluation));
        });

        api.MapPost("/sessions/{id:guid}/quiz", async (
            Guid id,
            BridgeDbContext db,
            QuizGenPromptProvider promptProvider,
            AdaptiveQuizService quizService,
            IAiClient ai,
            Microsoft.Extensions.Options.IOptions<OpenAiOptions> aiOptions,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            const int requestedCount = 5;
            var session = await db.PracticeSessions
                .Include(x => x.Actor)
                .Include(x => x.Scenario)
                .Include(x => x.Evaluation)
                .Include(x => x.Messages)
                .Include(x => x.User).ThenInclude(x => x.Profile)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session?.User.Profile is null) return Results.NotFound();
            if (session.Evaluation is null)
                return Results.Conflict(new { error = "Complete and evaluate the chat before generating a quiz." });

            AdaptiveQuizSnapshot? snapshot = null;
            if (!string.IsNullOrWhiteSpace(session.GeneratedQuizJson))
            {
                try
                {
                    snapshot = JsonSerializer.Deserialize<AdaptiveQuizSnapshot>(
                        session.GeneratedQuizJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException)
                {
                    snapshot = null;
                }
            }

            if (snapshot is null)
            {
                snapshot = quizService.CreateStaticFallback(session.Actor.ActorType);
                var systemPrompt = promptProvider.GetSystemPrompt();

                if (!string.IsNullOrWhiteSpace(systemPrompt) && !string.IsNullOrWhiteSpace(aiOptions.Value.ApiKey))
                {
                    try
                    {
                        var latestPersonaJson = await db.PersonaAnalyses.AsNoTracking()
                            .Where(x => x.UserId == session.UserId)
                            .OrderByDescending(x => x.CreatedAt)
                            .Select(x => x.AnalysisJson)
                            .FirstOrDefaultAsync(ct);

                        var previousQuestionJson = await db.QuizAttempts.AsNoTracking()
                            .Where(x => x.UserId == session.UserId)
                            .OrderByDescending(x => x.CreatedAt)
                            .Select(x => x.QuestionsJson)
                            .Take(5)
                            .ToListAsync(ct);
                        var previousQuestionCodes = new HashSet<string>(StringComparer.Ordinal);
                        foreach (var json in previousQuestionJson)
                        {
                            try
                            {
                                var previousQuestions = JsonSerializer.Deserialize<List<QuizQuestion>>(json) ?? [];
                                foreach (var code in previousQuestions.Select(x => x.QuestionCode).Where(x => !string.IsNullOrWhiteSpace(x)))
                                    previousQuestionCodes.Add(code!);
                            }
                            catch (JsonException)
                            {
                                // Older quiz snapshots may not contain the current shape.
                            }
                        }

                        var staticQuestionBank = QuizCatalog.ForActor(session.Actor.ActorType);
                        var input = JsonSerializer.Serialize(new
                        {
                            requested_question_count = requestedCount,
                            persona_profile = ParsePersona(latestPersonaJson ?? "") ?? new
                            {
                                primary_goal = session.User.Profile.HardestActorType,
                                confidence_baseline = session.User.Profile.BaselineConfidence,
                                confidence_current = session.User.Profile.CurrentConfidence,
                                classroom_comfort = session.User.Profile.ClassroomComfort,
                                disagreement_comfort = session.User.Profile.DisagreementComfort,
                                small_talk_comfort = session.User.Profile.SmallTalkComfort
                            },
                            chat_transcript = string.Join(
                                "\n",
                                session.Messages.OrderBy(x => x.SequenceNumber).Select(x => x.Role + ": " + x.Content)),
                            session_evaluation = ToEvaluation(session.Evaluation),
                            scenario = new
                            {
                                actor_type = session.Actor.ActorType,
                                session.Scenario.Title,
                                session.Scenario.Goal,
                                session.Scenario.CultureContext
                            },
                            quiz_question_bank = staticQuestionBank.Select((question, index) => new
                            {
                                question_code = "STATIC-BANK-" + (index + 1),
                                prompt = question.Prompt,
                                options = question.Options.Select((text, optionIndex) => new
                                {
                                    key = ((char)('A' + optionIndex)).ToString(),
                                    text
                                }),
                                correct_option = ((char)('A' + question.CorrectIndex)).ToString(),
                                explanation = question.Explanation,
                                skill_tag = question.SkillTag
                            }),
                            previously_seen_question_codes = previousQuestionCodes
                        });

                        var raw = await ai.GenerateAsync(systemPrompt, input, ct);
                        snapshot = quizService.ParseAndValidate(raw, requestedCount);
                    }
                    catch (Exception exception)
                    {
                        loggerFactory.CreateLogger("QuizGeneration")
                            .LogError(exception, "Adaptive quiz generation failed for session {SessionId}; using static fallback.", session.Id);
                    }
                }

                session.GeneratedQuizJson = JsonSerializer.Serialize(snapshot);
                session.QuizGenerationModel = snapshot.Source == "ai" ? aiOptions.Value.Model : null;
                session.QuizGenerationPromptVersion = snapshot.Source == "ai" ? promptProvider.PromptVersion : null;
                session.QuizGeneratedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
            }

            var studentQuiz = quizService.CreateStudentResponse(snapshot);
            return Results.Ok(studentQuiz.Questions.Select((question, index) => new
            {
                id = index,
                question.QuestionCode,
                question.Prompt,
                options = question.Options.Select(option => option.Text),
                question.SkillTag,
                question.Difficulty,
                question.ScenarioContext,
                question.IsReinforcement,
                source = studentQuiz.Source
            }));
        });

        api.MapPost("/sessions/{id:guid}/quiz/submit", async (
            Guid id,
            SubmitQuizRequest request,
            BridgeDbContext db,
            AdaptiveQuizService quizService,
            CancellationToken ct) =>
        {
            var session = await db.PracticeSessions
                .Include(x => x.User).ThenInclude(x => x.Profile)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session?.User.Profile is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(session.GeneratedQuizJson))
                return Results.Conflict(new { error = "Generate the quiz before submitting answers." });
            if (request.ConfidenceAfter is < 1 or > 5)
                return Results.BadRequest(new { error = "ConfidenceAfter must be between 1 and 5." });

            AdaptiveQuizSnapshot snapshot;
            AdaptiveQuizScoreResult result;
            try
            {
                snapshot = JsonSerializer.Deserialize<AdaptiveQuizSnapshot>(
                    session.GeneratedQuizJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Stored quiz is invalid.");
                result = quizService.Score(snapshot, request.Answers);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (JsonException)
            {
                return Results.Conflict(new { error = "Stored quiz is invalid. Generate it again." });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }

            var before = session.User.Profile.CurrentConfidence;
            session.User.Profile.CurrentConfidence = request.ConfidenceAfter;
            session.User.Profile.UpdatedAt = DateTimeOffset.UtcNow;
            db.QuizAttempts.Add(new QuizAttempt
            {
                UserId = session.UserId,
                SessionId = session.Id,
                QuestionsJson = JsonSerializer.Serialize(result.Questions),
                AnswersJson = JsonSerializer.Serialize(request.Answers),
                Score = result.Score,
                MaxScore = result.MaxScore,
                ConfidenceBefore = before,
                ConfidenceAfter = request.ConfidenceAfter
            });
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                score = result.Score,
                maxScore = result.MaxScore,
                confidenceBefore = before,
                confidenceAfter = request.ConfidenceAfter,
                knowledgeImproved = result.Score >= Math.Ceiling(result.MaxScore * 0.6),
                confidenceIncreased = request.ConfidenceAfter > before,
                answers = result.Questions.Select((question, index) => new
                {
                    correct = request.Answers[index] == question.CorrectIndex,
                    question.CorrectIndex,
                    question.Explanation,
                    question.QuestionCode,
                    question.SkillTag
                })
            });
        });

        // Post-quiz AI evaluation: combine the quiz result with the persona profile
        // and prior sessions, then route the next session. Runs only when the
        // evaluator prompt and an API key are configured; otherwise returns pending.
        api.MapPost("/sessions/{id:guid}/quiz/evaluation", async (
            Guid id,
            BridgeDbContext db,
            PostQuizPromptProvider promptProvider,
            IAiClient ai,
            Microsoft.Extensions.Options.IOptions<OpenAiOptions> aiOptions,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var session = await db.PracticeSessions
                .Include(x => x.Actor)
                .Include(x => x.User).ThenInclude(x => x.Profile)
                .Include(x => x.Evaluation)
                .Include(x => x.QuizAttempts)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session?.User.Profile is null) return Results.NotFound();

            var attempt = session.QuizAttempts.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
            if (attempt is null) return Results.BadRequest(new { error = "Complete a quiz before evaluation." });
            if (attempt.EvaluationJson is not null)
                return Results.Ok(new { status = "analyzed", evaluation = ParsePersona(attempt.EvaluationJson) });

            var systemPrompt = promptProvider.GetSystemPrompt();
            if (string.IsNullOrWhiteSpace(systemPrompt) || string.IsNullOrWhiteSpace(aiOptions.Value.ApiKey))
                return Results.Ok(new { status = "pending" });

            var profile = session.User.Profile;
            var questions = JsonSerializer.Deserialize<List<QuizQuestion>>(attempt.QuestionsJson) ?? [];
            var answers = JsonSerializer.Deserialize<List<int>>(attempt.AnswersJson) ?? [];
            var priorSessions = await db.PracticeSessions.AsNoTracking()
                .Where(x => x.UserId == session.UserId && x.Id != session.Id && x.Evaluation != null)
                .Include(x => x.Evaluation)
                .OrderByDescending(x => x.CompletedAt).Take(5)
                .ToListAsync(ct);

            var input = JsonSerializer.Serialize(new
            {
                derived_profile = new
                {
                    baseline_confidence = profile.BaselineConfidence,
                    current_confidence = profile.CurrentConfidence,
                    hardest_actor = profile.HardestActorType,
                    classroom_comfort = profile.ClassroomComfort,
                    disagreement_comfort = profile.DisagreementComfort,
                    small_talk_comfort = profile.SmallTalkComfort,
                    session_focus_skill = session.Evaluation is null ? null : LearningResourceCatalog.WeakestSkill(session.Evaluation)
                },
                quiz_result = new
                {
                    actor_type = session.Actor.ActorType,
                    score = attempt.Score,
                    max_score = attempt.MaxScore,
                    questions = questions.Select((q, index) => new
                    {
                        prompt = q.Prompt,
                        skill_tag = q.SkillTag,
                        selected_answer = index < answers.Count && answers[index] >= 0 && answers[index] < q.Options.Count ? q.Options[answers[index]] : null,
                        correct_answer = q.Options.Count > q.CorrectIndex ? q.Options[q.CorrectIndex] : null,
                        is_correct = index < answers.Count && answers[index] == q.CorrectIndex,
                        explanation = q.Explanation
                    })
                },
                previous_session_data = priorSessions.Select(x => new
                {
                    overall_score = x.Evaluation!.OverallScore,
                    weak_skill = LearningResourceCatalog.WeakestSkill(x.Evaluation),
                    completed_at = x.CompletedAt
                })
            });

            try
            {
                var raw = await ai.GenerateAsync(systemPrompt, input, ct);
                var evaluationJson = ExtractJson(raw);
                attempt.EvaluationJson = evaluationJson;
                attempt.EvaluationModel = aiOptions.Value.Model;
                attempt.EvaluationPromptVersion = promptProvider.PromptVersion;
                attempt.EvaluatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { status = "analyzed", evaluation = ParsePersona(evaluationJson) });
            }
            catch (Exception exception)
            {
                loggerFactory.CreateLogger("PostQuizEvaluation")
                    .LogError(exception, "Post-quiz evaluation failed for session {SessionId}", session.Id);
                return Results.Ok(new { status = "failed" });
            }
        });

        api.MapGet("/progress/{userId:guid}", async (Guid userId, BridgeDbContext db, CancellationToken ct) =>
        {
            var user = await db.Users.AsNoTracking().Include(x => x.Profile)
                .SingleOrDefaultAsync(x => x.Id == userId, ct);
            if (user?.Profile is null) return Results.NotFound();

            var sessions = await db.PracticeSessions.AsNoTracking()
                .Where(x => x.UserId == userId && x.Status == "completed" && x.Evaluation != null)
                .Include(x => x.Evaluation)
                .Include(x => x.Actor)
                .Include(x => x.Scenario)
                .ToListAsync(ct);
            var quizzes = await db.QuizAttempts.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(ct);
            var recentSessions = sessions.OrderByDescending(x => x.CompletedAt).Take(6).Select(x => new
            {
                x.Id,
                scenario = x.Scenario.Title,
                actor = x.Actor.Name,
                actorType = x.Actor.ActorType,
                score = x.Evaluation!.OverallScore,
                focusSkill = LearningResourceCatalog.WeakestSkill(x.Evaluation),
                x.CompletedAt
            });
            var latest = sessions.OrderByDescending(x => x.CompletedAt).FirstOrDefault();

            return Results.Ok(new
            {
                user.Id,
                user.DisplayName,
                user.Profile.BaselineConfidence,
                user.Profile.CurrentConfidence,
                user.Profile.HardestActorType,
                currentFocus = latest?.Evaluation is null ? null : LearningResourceCatalog.WeakestSkill(latest.Evaluation),
                completedSessions = sessions.Count,
                averageScore = sessions.Count == 0 ? 0 : Math.Round(sessions.Average(x => x.Evaluation!.OverallScore), 1),
                byActor = sessions.GroupBy(x => x.Actor.ActorType).Select(group => new
                {
                    actorType = group.Key,
                    count = group.Count(),
                    average = Math.Round(group.Average(x => x.Evaluation!.OverallScore), 1)
                }),
                quizAttempts = quizzes.Count,
                recentSessions
            });
        });

        api.MapGet("/sessions/{id:guid}/learning-plan", async (Guid id, BridgeDbContext db, CancellationToken ct) =>
        {
            var session = await db.PracticeSessions.AsNoTracking()
                .Include(x => x.Actor)
                .Include(x => x.Evaluation)
                .Include(x => x.QuizAttempts)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session?.Evaluation is null) return Results.NotFound();

            var skill = LearningResourceCatalog.WeakestSkill(session.Evaluation);
            var resources = LearningResourceCatalog.For(skill);
            var quiz = QuizCatalog.ForActor(session.Actor.ActorType)
                .Select((x, index) => new { id = index, x.Prompt, x.Options });
            return Results.Ok(new
            {
                focusSkill = skill,
                focusScore = ScoreFor(session.Evaluation, skill),
                resources,
                quiz,
                quizCompleted = session.QuizAttempts.Count != 0,
                nextStep = session.QuizAttempts.Count == 0 ? "complete_quiz" : "practice_again"
            });
        });

        return app;
    }

    private static int Clamp(int value) => Math.Clamp(value, 1, 5);

    private static string ExtractJson(string raw)
    {
        var text = raw.Trim();
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = text.IndexOf('\n');
            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (firstLineEnd >= 0 && lastFence > firstLineEnd)
                text = text[(firstLineEnd + 1)..lastFence].Trim();
        }
        return text;
    }

    private static string? TryReadSummary(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
            foreach (var key in new[] { "profile_summary", "summary" })
                if (doc.RootElement.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                    return value.GetString();
        }
        catch (JsonException) { /* non-JSON output is stored raw */ }
        return null;
    }

    private static object? ParsePersona(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? MapRecommendedActor(string? recommended) => recommended?.Trim().ToLowerInvariant() switch
    {
        "professor" or "advisor" => "professor",
        "classmate" or "group_member" or "peer" => "friend",
        _ => null
    };

    private static string? MapGoalToActor(string? goal) => goal switch
    {
        "professor-communication" or "disagree-feedback" => "professor",
        "speak-up-confidence" or "group-project" or "general-fluency" => "friend",
        _ => null
    };

    private static int? TryReadInt(string? json, string property)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(property, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)) return number;
                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed)) return parsed;
            }
        }
        catch (JsonException) { }
        return null;
    }

    private static string? TryReadNestedString(string? json, string outerKey, string innerKey)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty(outerKey, out var outer) && outer.ValueKind == JsonValueKind.Object &&
                outer.TryGetProperty(innerKey, out var inner) && inner.ValueKind == JsonValueKind.String)
                return inner.GetString();
        }
        catch (JsonException) { }
        return null;
    }

    private static int ScoreFor(SessionEvaluation evaluation, string skill) => skill switch
    {
        "clarity" => evaluation.ClarityScore,
        "directness" => evaluation.DirectnessScore,
        "warmth" => evaluation.WarmthScore,
        "engagement" => evaluation.EngagementScore,
        _ => evaluation.GoalCompletionScore
    };

    private static object ToEvaluation(SessionEvaluation evaluation) => new
    {
        evaluation.Id,
        evaluation.SessionId,
        evaluation.OverallScore,
        dimensions = new
        {
            clarity = evaluation.ClarityScore,
            directness = evaluation.DirectnessScore,
            warmth = evaluation.WarmthScore,
            engagement = evaluation.EngagementScore,
            goalCompletion = evaluation.GoalCompletionScore
        },
        strengths = JsonSerializer.Deserialize<IReadOnlyList<FeedbackItem>>(evaluation.StrengthsJson) ?? [],
        improvements = JsonSerializer.Deserialize<IReadOnlyList<FeedbackItem>>(evaluation.ImprovementsJson) ?? [],
        cultureGap = JsonSerializer.Deserialize<CultureGap>(evaluation.CultureGapJson),
        evaluation.Summary
    };
}