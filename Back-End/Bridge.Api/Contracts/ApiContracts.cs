using System.Text.Json.Serialization;

namespace Bridge.Api.Contracts;

public sealed record CreateAnonymousUserRequest(string? DisplayName);
public sealed record OnboardingAssessmentRequest(string DisplayName, string? CountryOfOrigin, string UsExperience, int ClassroomComfort, int DisagreementComfort, int SmallTalkComfort, string HardestActorType);
public sealed record OnboardingAssessmentResult(int BaselineConfidence, string RecommendedActorType);
public sealed record SaveOnboardingRequest(Guid UserId, OnboardingAssessmentRequest Assessment);
public sealed record CreateSessionRequest(Guid UserId, string ActorId, string ScenarioId, int DirectnessLevel);
public sealed record SendMessageRequest(Guid ClientMessageId, string Content);
public sealed record CompleteSessionRequest(bool Confirmed);
public sealed record SubmitQuizRequest(IReadOnlyList<int> Answers, int ConfidenceAfter);
public sealed record ChatAiResult(string Reply, string? CultureNoteType, string? CultureNote, int GoalProgress);
public sealed record FeedbackItem(string Title, string Evidence, string? Suggestion);
public sealed record CultureGap(string Original, string Alternative, string Explanation);
public sealed record EvaluationAiResult(int OverallScore, int Clarity, int Directness, int Warmth, int Engagement, int GoalCompletion, IReadOnlyList<FeedbackItem> Strengths, IReadOnlyList<FeedbackItem> Improvements, CultureGap CultureGap, [property: JsonConverter(typeof(FlexibleSummaryJsonConverter))] string Summary);
public sealed record QuizQuestion(string Prompt, IReadOnlyList<string> Options, int CorrectIndex, string Explanation);

// Persona assessment submission.
public sealed record PersonaAnswerInput(string QuestionId, IReadOnlyList<string>? SelectedValues, int? ScaleValue);
public sealed record SubmitPersonaAssessmentRequest(Guid UserId, IReadOnlyList<PersonaAnswerInput> Answers);
