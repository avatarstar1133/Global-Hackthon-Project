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
        api.MapPost("/users/anonymous", async (CreateAnonymousUserRequest r, BridgeDbContext db, CancellationToken ct) => { var u = new AppUser { DisplayName = string.IsNullOrWhiteSpace(r.DisplayName) ? "Learner" : r.DisplayName.Trim() }; db.Users.Add(u); await db.SaveChangesAsync(ct); return Results.Created($"/api/users/${u.Id}", new { u.Id, u.DisplayName }); });
        api.MapPost("/onboarding/assess", async (SaveOnboardingRequest r, BridgeDbContext db, OnboardingAssessmentService service, CancellationToken ct) => { var u = await db.Users.Include(x => x.Profile).SingleOrDefaultAsync(x => x.Id == r.UserId, ct); if (u is null) return Results.NotFound(); OnboardingAssessmentResult a; try { a = service.Assess(r.Assessment); } catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); } u.DisplayName = r.Assessment.DisplayName.Trim(); u.Profile ??= new UserProfile { UserId = u.Id }; var p = u.Profile; p.CountryOfOrigin = r.Assessment.CountryOfOrigin; p.UsExperience = r.Assessment.UsExperience; p.BaselineConfidence = a.BaselineConfidence; p.CurrentConfidence = a.BaselineConfidence; p.HardestActorType = a.RecommendedActorType; p.ClassroomComfort = r.Assessment.ClassroomComfort; p.DisagreementComfort = r.Assessment.DisagreementComfort; p.SmallTalkComfort = r.Assessment.SmallTalkComfort; p.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Results.Ok(a); });
        api.MapGet("/actors", async (BridgeDbContext db, CancellationToken ct) => Results.Ok(await db.Actors.AsNoTracking().Where(x => x.IsActive).Select(x => new { x.Id, x.ActorType, x.Name, x.Role, x.Personality, x.CommunicationStyle }).ToListAsync(ct)));
        api.MapGet("/scenarios", async (string? actor, BridgeDbContext db, CancellationToken ct) => Results.Ok(await db.Scenarios.AsNoTracking().Where(x => x.IsActive && (actor == null || x.Actor.ActorType == actor)).Select(x => new { x.Id, x.ActorId, x.Actor.ActorType, x.Title, x.Description, x.Goal, x.Difficulty, x.OpeningMessage, x.CultureContext }).ToListAsync(ct)));
        api.MapPost("/sessions", async (CreateSessionRequest r, BridgeDbContext db, CancellationToken ct) =>
        {
            if (r.DirectnessLevel is < 1 or > 5) return Results.BadRequest(new { error = "DirectnessLevel must be 1-5." });
            var user = await db.Users.Include(x => x.Profile).SingleOrDefaultAsync(x => x.Id == r.UserId, ct); var actor = await db.Actors.FindAsync([r.ActorId], ct); var scenario = await db.Scenarios.FindAsync([r.ScenarioId], ct);
            if (user?.Profile is null || actor is null || scenario is null || scenario.ActorId != actor.Id) return Results.BadRequest(new { error = "Invalid user profile, actor or scenario." });
            var s = new PracticeSession { UserId = user.Id, ActorId = actor.Id, ScenarioId = scenario.Id, DirectnessLevel = r.DirectnessLevel }; s.Messages.Add(new ChatMessage { Role = "actor", Content = scenario.OpeningMessage, SequenceNumber = 1 }); db.PracticeSessions.Add(s); await db.SaveChangesAsync(ct); return Results.Created($"/api/sessions/${s.Id}", new { s.Id, s.Status, actor = new { actor.Id, actor.Name, actor.Role }, scenario = new { scenario.Id, scenario.Title, scenario.Goal }, scenario.OpeningMessage });
        });
        api.MapGet("/sessions/{id:guid}", async (Guid id, BridgeDbContext db, CancellationToken ct) => { var s = await db.PracticeSessions.AsNoTracking().Include(x => x.Actor).Include(x => x.Scenario).Include(x => x.Messages).Include(x => x.Evaluation).SingleOrDefaultAsync(x => x.Id == id, ct); return s is null ? Results.NotFound() : Results.Ok(new { s.Id, s.UserId, s.Status, s.DirectnessLevel, actor = new { s.Actor.Id, s.Actor.Name, s.Actor.Role }, scenario = new { s.Scenario.Id, s.Scenario.Title, s.Scenario.Goal }, messages = s.Messages.OrderBy(x => x.SequenceNumber).Select(x => new { x.Id, x.Role, x.Content, x.GoalProgress, x.CultureNoteType, x.CultureNote, x.CreatedAt }), evaluation = s.Evaluation }); });
        api.MapPost("/sessions/{id:guid}/messages", async (Guid id, SendMessageRequest r, BridgeDbContext db, PromptBuilder prompts, IAiClient ai, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(r.Content) || r.Content.Length > 2000) return Results.BadRequest(new { error = "Message must be 1-2000 characters." });
            var s = await db.PracticeSessions.Include(x => x.Actor).Include(x => x.Scenario).Include(x => x.User).ThenInclude(x => x.Profile).Include(x => x.Messages).SingleOrDefaultAsync(x => x.Id == id, ct);
            if (s is null) return Results.NotFound(); if (s.Status != "active") return Results.Conflict(new { error = "Session is not active." });
            var existing = s.Messages.FirstOrDefault(x => x.ClientMessageId == r.ClientMessageId); if (existing is not null) return Results.Ok(new { duplicate = true, existing.Id });
            var next = s.Messages.Count == 0 ? 1 : s.Messages.Max(x => x.SequenceNumber) + 1; var userMessage = new ChatMessage { SessionId = s.Id, ClientMessageId = r.ClientMessageId, Role = "user", Content = r.Content.Trim(), SequenceNumber = next }; s.Messages.Add(userMessage); await db.SaveChangesAsync(ct);
            var transcript = string.Join("\n", s.Messages.OrderBy(x => x.SequenceNumber).Select(x => x.Role + ": " + x.Content)); var raw = await ai.GenerateAsync(prompts.BuildRolePlayPrompt(s.Actor, s.Scenario, s.User.Profile!, s.DirectnessLevel), transcript, ct); var result = AiJson.Parse<ChatAiResult>(raw);
            var actorMessage = new ChatMessage { SessionId = s.Id, Role = "actor", Content = result.Reply, SequenceNumber = next + 1, GoalProgress = Math.Clamp(result.GoalProgress, 0, 100), CultureNoteType = result.CultureNoteType, CultureNote = result.CultureNote }; s.Messages.Add(actorMessage); await db.SaveChangesAsync(ct); return Results.Ok(new { messageId = actorMessage.Id, result.Reply, result.CultureNoteType, result.CultureNote, goalProgress = actorMessage.GoalProgress });
        });
        api.MapPost("/sessions/{id:guid}/hint", async (Guid id, BridgeDbContext db, IAiClient ai, CancellationToken ct) => { var s = await db.PracticeSessions.Include(x => x.Scenario).Include(x => x.Messages).SingleOrDefaultAsync(x => x.Id == id, ct); if (s is null) return Results.NotFound(); var last = s.Messages.OrderBy(x => x.SequenceNumber).LastOrDefault()?.Content ?? ""; var hint = await ai.GenerateAsync("Give one short intent hint or sentence starter. Do not write the full answer.", $"Goal: ${s.Scenario.Goal}\nLast turn: ${last}", ct); return Results.Ok(new { hint }); });
        api.MapPost("/sessions/{id:guid}/complete", async (Guid id, CompleteSessionRequest r, BridgeDbContext db, PromptBuilder prompts, IAiClient ai, Microsoft.Extensions.Options.IOptions<OpenAiOptions> options, CancellationToken ct) =>
        {
            if (!r.Confirmed) return Results.BadRequest(new { error = "Completion must be confirmed." }); var s = await db.PracticeSessions.Include(x => x.Actor).Include(x => x.Scenario).Include(x => x.User).ThenInclude(x => x.Profile).Include(x => x.Messages).Include(x => x.Evaluation).SingleOrDefaultAsync(x => x.Id == id, ct); if (s is null) return Results.NotFound(); if (s.Evaluation is not null) return Results.Ok(ToEvaluation(s.Evaluation)); if (!s.Messages.Any(x => x.Role == "user")) return Results.BadRequest(new { error = "Send at least one message before completing." });
            var raw = await ai.GenerateAsync(prompts.BuildEvaluationPrompt(s.Actor, s.Scenario, s.User.Profile!, s.Messages), "Evaluate the transcript now.", ct); var e = AiJson.Parse<EvaluationAiResult>(raw); var entity = new SessionEvaluation { SessionId = s.Id, OverallScore = Clamp(e.OverallScore), ClarityScore = Clamp(e.Clarity), DirectnessScore = Clamp(e.Directness), WarmthScore = Clamp(e.Warmth), EngagementScore = Clamp(e.Engagement), GoalCompletionScore = Clamp(e.GoalCompletion), StrengthsJson = JsonSerializer.Serialize(e.Strengths), ImprovementsJson = JsonSerializer.Serialize(e.Improvements), CultureGapJson = JsonSerializer.Serialize(e.CultureGap), Summary = e.Summary, ModelName = options.Value.Model }; s.Evaluation = entity; s.Status = "completed"; s.CompletedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Results.Ok(ToEvaluation(entity));
        });
        api.MapPost("/sessions/{id:guid}/quiz", async (Guid id, BridgeDbContext db, CancellationToken ct) => { var s = await db.PracticeSessions.Include(x => x.Actor).SingleOrDefaultAsync(x => x.Id == id, ct); if (s is null) return Results.NotFound(); var q = QuizCatalog.ForActor(s.Actor.ActorType); return Results.Ok(q.Select((x, i) => new { id = i, x.Prompt, x.Options })); });
        api.MapPost("/sessions/{id:guid}/quiz/submit", async (Guid id, SubmitQuizRequest r, BridgeDbContext db, CancellationToken ct) => { var s = await db.PracticeSessions.Include(x => x.Actor).Include(x => x.User).ThenInclude(x => x.Profile).SingleOrDefaultAsync(x => x.Id == id, ct); if (s?.User.Profile is null) return Results.NotFound(); var q = QuizCatalog.ForActor(s.Actor.ActorType); if (r.Answers.Count != q.Count || r.ConfidenceAfter is < 1 or > 5) return Results.BadRequest(new { error = "Invalid quiz submission." }); var score = q.Select((x, i) => x.CorrectIndex == r.Answers[i] ? 1 : 0).Sum(); var before = s.User.Profile.CurrentConfidence; s.User.Profile.CurrentConfidence = r.ConfidenceAfter; s.User.Profile.UpdatedAt = DateTimeOffset.UtcNow; var attempt = new QuizAttempt { UserId = s.UserId, SessionId = s.Id, QuestionsJson = JsonSerializer.Serialize(q), AnswersJson = JsonSerializer.Serialize(r.Answers), Score = score, MaxScore = q.Count, ConfidenceBefore = before, ConfidenceAfter = r.ConfidenceAfter }; db.QuizAttempts.Add(attempt); await db.SaveChangesAsync(ct); return Results.Ok(new { score, maxScore = q.Count, confidenceBefore = before, confidenceAfter = r.ConfidenceAfter, knowledgeImproved = score >= 2, confidenceIncreased = r.ConfidenceAfter > before, answers = q.Select((x, i) => new { correct = r.Answers[i] == x.CorrectIndex, x.CorrectIndex, x.Explanation }) }); });
        api.MapGet("/progress/{userId:guid}", async (Guid userId, BridgeDbContext db, CancellationToken ct) => { var user = await db.Users.AsNoTracking().Include(x => x.Profile).SingleOrDefaultAsync(x => x.Id == userId, ct); if (user?.Profile is null) return Results.NotFound(); var sessions = await db.PracticeSessions.AsNoTracking().Where(x => x.UserId == userId && x.Status == "completed").Include(x => x.Evaluation).Include(x => x.Actor).ToListAsync(ct); var quizzes = await db.QuizAttempts.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(ct); return Results.Ok(new { user.Id, user.DisplayName, user.Profile.BaselineConfidence, user.Profile.CurrentConfidence, user.Profile.HardestActorType, completedSessions = sessions.Count, averageScore = sessions.Count == 0 ? 0 : Math.Round(sessions.Average(x => x.Evaluation!.OverallScore), 1), byActor = sessions.GroupBy(x => x.Actor.ActorType).Select(g => new { actorType = g.Key, count = g.Count(), average = Math.Round(g.Average(x => x.Evaluation!.OverallScore), 1) }), quizAttempts = quizzes.Count }); });
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
                focusScore = skill switch
                {
                    "clarity" => session.Evaluation.ClarityScore,
                    "directness" => session.Evaluation.DirectnessScore,
                    "warmth" => session.Evaluation.WarmthScore,
                    "engagement" => session.Evaluation.EngagementScore,
                    _ => session.Evaluation.GoalCompletionScore
                },
                resources,
                quiz,
                quizCompleted = session.QuizAttempts.Count != 0,
                nextStep = session.QuizAttempts.Count == 0 ? "complete_quiz" : "practice_again"
            });
        });

        return app;
    }
    private static int Clamp(int value) => Math.Clamp(value, 1, 5);
    private static object ToEvaluation(SessionEvaluation e) => new { e.Id, e.SessionId, e.OverallScore, dimensions = new { clarity = e.ClarityScore, directness = e.DirectnessScore, warmth = e.WarmthScore, engagement = e.EngagementScore, goalCompletion = e.GoalCompletionScore }, strengths = JsonSerializer.Deserialize<object>(e.StrengthsJson), improvements = JsonSerializer.Deserialize<object>(e.ImprovementsJson), cultureGap = JsonSerializer.Deserialize<object>(e.CultureGapJson), e.Summary };
}
