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
            session.Messages.Add(userMessage);
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
            session.Messages.Add(actorMessage);
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
            session.Evaluation = evaluation;
            session.Status = "completed";
            session.CompletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToEvaluation(evaluation));
        });

        api.MapPost("/sessions/{id:guid}/quiz", async (Guid id, BridgeDbContext db, CancellationToken ct) =>
        {
            var session = await db.PracticeSessions.Include(x => x.Actor)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session is null) return Results.NotFound();
            var questions = QuizCatalog.ForActor(session.Actor.ActorType);
            return Results.Ok(questions.Select((x, index) => new { id = index, x.Prompt, x.Options }));
        });

        api.MapPost("/sessions/{id:guid}/quiz/submit", async (Guid id, SubmitQuizRequest request, BridgeDbContext db, CancellationToken ct) =>
        {
            var session = await db.PracticeSessions.Include(x => x.Actor).Include(x => x.User).ThenInclude(x => x.Profile)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session?.User.Profile is null) return Results.NotFound();
            var questions = QuizCatalog.ForActor(session.Actor.ActorType);
            if (request.Answers.Count != questions.Count || request.ConfidenceAfter is < 1 or > 5)
                return Results.BadRequest(new { error = "Invalid quiz submission." });

            var score = questions.Select((x, index) => x.CorrectIndex == request.Answers[index] ? 1 : 0).Sum();
            var before = session.User.Profile.CurrentConfidence;
            session.User.Profile.CurrentConfidence = request.ConfidenceAfter;
            session.User.Profile.UpdatedAt = DateTimeOffset.UtcNow;
            db.QuizAttempts.Add(new QuizAttempt
            {
                UserId = session.UserId,
                SessionId = session.Id,
                QuestionsJson = JsonSerializer.Serialize(questions),
                AnswersJson = JsonSerializer.Serialize(request.Answers),
                Score = score,
                MaxScore = questions.Count,
                ConfidenceBefore = before,
                ConfidenceAfter = request.ConfidenceAfter
            });
            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                score,
                maxScore = questions.Count,
                confidenceBefore = before,
                confidenceAfter = request.ConfidenceAfter,
                knowledgeImproved = score >= 2,
                confidenceIncreased = request.ConfidenceAfter > before,
                answers = questions.Select((x, index) => new
                {
                    correct = request.Answers[index] == x.CorrectIndex,
                    x.CorrectIndex,
                    x.Explanation
                })
            });
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