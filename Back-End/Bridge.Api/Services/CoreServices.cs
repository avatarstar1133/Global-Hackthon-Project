using Bridge.Api.Contracts;
using Bridge.Api.Domain;

namespace Bridge.Api.Services;

public sealed class OnboardingAssessmentService
{
    private static readonly HashSet<string> Types = new(StringComparer.OrdinalIgnoreCase)
        { "friend", "professor" };

    public OnboardingAssessmentResult Assess(OnboardingAssessmentRequest request)
    {
        foreach (var score in new[] { request.ClassroomComfort, request.DisagreementComfort, request.SmallTalkComfort })
            if (score is < 1 or > 5)
                throw new ArgumentOutOfRangeException(nameof(request), "Comfort scores must be between 1 and 5.");

        if (!Types.Contains(request.HardestActorType))
            throw new ArgumentException("Unknown actor type.", nameof(request));

        var confidence = (int)Math.Round(
            (request.ClassroomComfort + request.DisagreementComfort + request.SmallTalkComfort) / 3d,
            MidpointRounding.AwayFromZero);

        return new(confidence, request.HardestActorType.ToLowerInvariant());
    }
}

public sealed class PromptBuilder
{
    public string BuildRolePlayPrompt(Actor actor, Scenario scenario, UserProfile profile, int directnessLevel)
    {
        if (directnessLevel is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(directnessLevel));

        return string.Join(Environment.NewLine,
            "You are a role-play partner helping an international student practice communication in common United States settings.",
            "Stay in character. Be realistic and psychologically safe. Never diagnose the learner or stereotype any culture.",
            "Keep responses concise, ask at most one question, and do not coach inside character dialogue.",
            $"Character: {actor.Name}, {actor.Role}. Personality: {actor.Personality}. Style: {actor.CommunicationStyle}",
            $"Scenario: {scenario.Title}. Situation: {scenario.Description}",
            $"Learner goal: {scenario.Goal}",
            $"Scenario behavior: {scenario.PromptInstructions}",
            $"Directness level: {directnessLevel} out of 5.",
            "At 1 gently clarify indirect statements; at 3 act typically supportive; at 5 expect clear communication without becoming rude.",
            $"Learner confidence: {profile.CurrentConfidence}/5. Focus: communication with {profile.HardestActorType}s.",
            "Return only JSON with keys: reply, cultureNoteType, cultureNote, goalProgress.",
            "goalProgress is an integer from 0 to 100.");
    }

    public string BuildEvaluationPrompt(
        Actor actor,
        Scenario scenario,
        UserProfile profile,
        IReadOnlyList<ChatMessage> messages)
    {
        var transcript = string.Join(
            Environment.NewLine,
            messages.OrderBy(x => x.SequenceNumber).Select(x => $"{x.Role}: {x.Content}"));

        return string.Join(Environment.NewLine,
            "You are an intercultural communication coach evaluating a completed role-play.",
            "Use only transcript evidence. Do not diagnose personality, anxiety or mental health.",
            "Ignore minor grammar unless meaning is blocked.",
            $"Actor: {actor.Name} ({actor.ActorType}).",
            $"Scenario: {scenario.Title}. Goal: {scenario.Goal}.",
            $"Baseline confidence: {profile.BaselineConfidence}/5.",
            "Transcript:",
            transcript,
            "Return only JSON with integer scores 1-5 and these keys:",
            "overallScore, clarity, directness, warmth, engagement, goalCompletion, strengths, improvements, cultureGap, summary.",
            "Each feedback item has title, evidence and optional suggestion.",
            "cultureGap has original, alternative and explanation.",
            "summary must be one concise plain-text string, never an object or array.");
    }
}
