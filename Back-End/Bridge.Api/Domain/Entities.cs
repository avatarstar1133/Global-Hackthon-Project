namespace Bridge.Api.Domain;

public sealed class AppUser { public Guid Id { get; set; } = Guid.NewGuid(); public string DisplayName { get; set; } = "Learner"; public bool IsAnonymous { get; set; } = true; public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; public UserProfile? Profile { get; set; } public List<PracticeSession> Sessions { get; set; } = []; }
public sealed class UserProfile { public Guid UserId { get; set; } public string? CountryOfOrigin { get; set; } public string UsExperience { get; set; } = ""; public int BaselineConfidence { get; set; } public int CurrentConfidence { get; set; } public string HardestActorType { get; set; } = "friend"; public int ClassroomComfort { get; set; } public int DisagreementComfort { get; set; } public int SmallTalkComfort { get; set; } public string AssessmentVersion { get; set; } = "v1"; public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow; public AppUser User { get; set; } = null!; }
public sealed class Actor { public string Id { get; set; } = ""; public string ActorType { get; set; } = ""; public string Name { get; set; } = ""; public string Role { get; set; } = ""; public string Personality { get; set; } = ""; public string CommunicationStyle { get; set; } = ""; public bool IsActive { get; set; } = true; public List<Scenario> Scenarios { get; set; } = []; }
public sealed class Scenario { public string Id { get; set; } = ""; public string ActorId { get; set; } = ""; public string Title { get; set; } = ""; public string Description { get; set; } = ""; public string Goal { get; set; } = ""; public string Difficulty { get; set; } = "medium"; public string OpeningMessage { get; set; } = ""; public string CultureContext { get; set; } = ""; public string PromptInstructions { get; set; } = ""; public bool IsActive { get; set; } = true; public Actor Actor { get; set; } = null!; }
public sealed class PracticeSession
{
    public Guid Id { get; set; } = Guid.NewGuid(); public Guid UserId { get; set; }
    public string ActorId { get; set; } = ""; public string ScenarioId { get; set; } = ""; public int DirectnessLevel { get; set; } = 3; public string Status { get; set; } = "active"; public string PromptVersion { get; set; } = "roleplay-v1"; public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow; public DateTimeOffset? CompletedAt { get; set; }
    // The quiz served for this session (AI-generated or static), stored so the
    // submit step scores against exactly what was shown. Null until a quiz is served.
    public string? GeneratedQuizJson { get; set; }
    public string? QuizGenerationModel { get; set; }
    public string? QuizGenerationPromptVersion { get; set; }
    public DateTimeOffset? QuizGeneratedAt { get; set; }
    public AppUser User { get; set; } = null!; public Actor Actor { get; set; } = null!; public Scenario Scenario { get; set; } = null!; public List<ChatMessage> Messages { get; set; } = []; public SessionEvaluation? Evaluation { get; set; }
    public List<QuizAttempt> QuizAttempts { get; set; } = [];
}
public sealed class ChatMessage { public Guid Id { get; set; } = Guid.NewGuid(); public Guid SessionId { get; set; } public Guid? ClientMessageId { get; set; } public string Role { get; set; } = ""; public string Content { get; set; } = ""; public int SequenceNumber { get; set; } public int? GoalProgress { get; set; } public string? CultureNoteType { get; set; } public string? CultureNote { get; set; } public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; public PracticeSession Session { get; set; } = null!; }
public sealed class SessionEvaluation { public Guid Id { get; set; } = Guid.NewGuid(); public Guid SessionId { get; set; } public int OverallScore { get; set; } public int ClarityScore { get; set; } public int DirectnessScore { get; set; } public int WarmthScore { get; set; } public int EngagementScore { get; set; } public int GoalCompletionScore { get; set; } public string StrengthsJson { get; set; } = "[]"; public string ImprovementsJson { get; set; } = "[]"; public string CultureGapJson { get; set; } = "{}"; public string Summary { get; set; } = ""; public string ModelName { get; set; } = ""; public string PromptVersion { get; set; } = "evaluation-v1"; public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; public PracticeSession Session { get; set; } = null!; }
public sealed class QuizAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid(); public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string QuestionsJson { get; set; } = "[]"; public string AnswersJson { get; set; } = "[]"; public int Score { get; set; }
    public int MaxScore { get; set; }
    public int ConfidenceBefore { get; set; }
    public int ConfidenceAfter { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    // Post-quiz AI evaluation (raw JSON per the evaluator prompt) — null until run.
    public string? EvaluationJson { get; set; }
    public string? EvaluationModel { get; set; }
    public string? EvaluationPromptVersion { get; set; }
    public DateTimeOffset? EvaluatedAt { get; set; }
    public AppUser User { get; set; } = null!; public PracticeSession Session { get; set; } = null!;
}

public sealed class AssessmentDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<AssessmentSection> Sections { get; set; } = [];
}

public sealed class AssessmentSection
{
    public string Id { get; set; } = "";
    public string AssessmentDefinitionId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int Order { get; set; }
    public AssessmentDefinition AssessmentDefinition { get; set; } = null!;
    public List<AssessmentQuestion> Questions { get; set; } = [];
}

public sealed class AssessmentQuestion
{
    public string Id { get; set; } = "";
    public string SectionId { get; set; } = "";
    public string Code { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string AnswerType { get; set; } = "single-choice";
    public int Order { get; set; }
    public int MinSelections { get; set; } = 1;
    public int MaxSelections { get; set; } = 1;
    public int? ScaleMin { get; set; }
    public int? ScaleMax { get; set; }
    public string? ScaleMinLabel { get; set; }
    public string? ScaleMaxLabel { get; set; }
    public bool IsRequired { get; set; } = true;
    public AssessmentSection Section { get; set; } = null!;
    public List<AssessmentOption> Options { get; set; } = [];
}

public sealed class AssessmentOption
{
    public string Id { get; set; } = "";
    public string QuestionId { get; set; } = "";
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
    public int Order { get; set; }
    public AssessmentQuestion Question { get; set; } = null!;
}

// One completed run of an assessment by a user (the survey they filled in).
public sealed class AssessmentAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string AssessmentId { get; set; } = "";
    public string AssessmentVersion { get; set; } = "";
    // pending | analyzed | failed  — reflects the AI persona analysis stage.
    public string AnalysisStatus { get; set; } = "pending";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public AppUser User { get; set; } = null!;
    public AssessmentDefinition Assessment { get; set; } = null!;
    public List<AssessmentResponse> Responses { get; set; } = [];
    public PersonaAnalysis? Analysis { get; set; }
}

// A single answer within an attempt.
public sealed class AssessmentResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttemptId { get; set; }
    public string QuestionId { get; set; } = "";
    public string QuestionCode { get; set; } = "";
    public string AnswerType { get; set; } = "";
    // For choice questions: JSON array of selected option values, e.g. ["A","C"].
    public string? SelectedValuesJson { get; set; }
    // For scale questions.
    public int? ScaleValue { get; set; }
    public AssessmentAttempt Attempt { get; set; } = null!;
    public AssessmentQuestion Question { get; set; } = null!;
}

// AI-produced persona profile derived from an attempt. The output schema is
// defined by the (user-provided) system prompt, so the raw JSON is stored as-is.
public sealed class PersonaAnalysis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttemptId { get; set; }
    public Guid UserId { get; set; }
    public string PromptVersion { get; set; } = "persona-analysis-v1";
    public string ModelName { get; set; } = "";
    public string AnalysisJson { get; set; } = "{}";
    public string? Summary { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public AssessmentAttempt Attempt { get; set; } = null!;
}
