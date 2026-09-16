using System.Text.Json.Serialization;

namespace Bridge.Api.Contracts;

public sealed record AdaptiveQuizOption(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("text")] string Text);

public sealed record AdaptiveQuizStudentQuestion(
    [property: JsonPropertyName("question_code")] string QuestionCode,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("options")] IReadOnlyList<AdaptiveQuizOption> Options,
    [property: JsonPropertyName("skill_tag")] string SkillTag,
    [property: JsonPropertyName("difficulty")] string Difficulty,
    [property: JsonPropertyName("scenario_context")] string ScenarioContext,
    [property: JsonPropertyName("is_reinforcement")] bool IsReinforcement);

public sealed record AdaptiveQuizAnswerKey(
    [property: JsonPropertyName("question_code")] string QuestionCode,
    [property: JsonPropertyName("correct_option")] string CorrectOption,
    [property: JsonPropertyName("explanation")] string Explanation,
    [property: JsonPropertyName("source_basis")] IReadOnlyList<string> SourceBasis);

public sealed record AdaptiveQuizAiResult(
    [property: JsonPropertyName("quiz_version")] string QuizVersion,
    [property: JsonPropertyName("quiz_title")] string QuizTitle,
    [property: JsonPropertyName("generation_reason")] string GenerationReason,
    [property: JsonPropertyName("target_skills")] IReadOnlyList<string> TargetSkills,
    [property: JsonPropertyName("student_questions")] IReadOnlyList<AdaptiveQuizStudentQuestion> StudentQuestions,
    [property: JsonPropertyName("answer_key")] IReadOnlyList<AdaptiveQuizAnswerKey> AnswerKey);

public sealed record AdaptiveQuizSnapshot(
    string QuizVersion,
    string QuizTitle,
    string GenerationReason,
    IReadOnlyList<string> TargetSkills,
    IReadOnlyList<AdaptiveQuizStudentQuestion> StudentQuestions,
    IReadOnlyList<AdaptiveQuizAnswerKey> AnswerKey,
    string Source);

public sealed record AdaptiveQuizStudentResponse(
    string QuizVersion,
    string QuizTitle,
    string GenerationReason,
    IReadOnlyList<string> TargetSkills,
    IReadOnlyList<AdaptiveQuizStudentQuestion> Questions,
    string Source);

public sealed record AdaptiveQuizScoreResult(
    int Score,
    int MaxScore,
    IReadOnlyList<QuizQuestion> Questions);
