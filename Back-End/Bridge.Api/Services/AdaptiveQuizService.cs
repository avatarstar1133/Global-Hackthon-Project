using Bridge.Api.Contracts;

namespace Bridge.Api.Services;

public sealed class AdaptiveQuizService
{
    private static readonly HashSet<string> AllowedDifficulties = new(StringComparer.Ordinal)
    {
        "easy", "medium", "hard"
    };

    public AdaptiveQuizSnapshot ParseAndValidate(string raw, int requestedCount)
    {
        if (requestedCount is < 5 or > 15)
            throw new ArgumentOutOfRangeException(nameof(requestedCount), "Quiz size must be between 5 and 15.");

        var result = AiJson.Parse<AdaptiveQuizAiResult>(raw);
        if (result.StudentQuestions is null || result.AnswerKey is null ||
            result.StudentQuestions.Count != requestedCount || result.AnswerKey.Count != requestedCount)
            throw new InvalidOperationException($"AI quiz must contain exactly {requestedCount} questions and answers.");

        var questionByCode = new Dictionary<string, AdaptiveQuizStudentQuestion>(StringComparer.Ordinal);
        foreach (var question in result.StudentQuestions)
        {
            if (string.IsNullOrWhiteSpace(question.QuestionCode) || !questionByCode.TryAdd(question.QuestionCode, question))
                throw new InvalidOperationException("AI quiz question codes must be non-empty and unique.");
            if (string.IsNullOrWhiteSpace(question.Prompt) || question.Options is null || question.Options.Count != 4)
                throw new InvalidOperationException($"Question '{question.QuestionCode}' must have a prompt and exactly four options.");
            if (!AllowedDifficulties.Contains(question.Difficulty))
                throw new InvalidOperationException($"Question '{question.QuestionCode}' has an invalid difficulty.");

            var optionKeys = question.Options.Select(option => option.Key).ToHashSet(StringComparer.Ordinal);
            if (optionKeys.Count != 4 || !optionKeys.SetEquals(["A", "B", "C", "D"]) ||
                question.Options.Any(option => string.IsNullOrWhiteSpace(option.Text)))
                throw new InvalidOperationException($"Question '{question.QuestionCode}' must have unique A-D options.");
        }

        var answerByCode = new Dictionary<string, AdaptiveQuizAnswerKey>(StringComparer.Ordinal);
        foreach (var answer in result.AnswerKey)
        {
            if (string.IsNullOrWhiteSpace(answer.QuestionCode) || !answerByCode.TryAdd(answer.QuestionCode, answer))
                throw new InvalidOperationException("AI answer-key codes must be non-empty and unique.");
            if (!questionByCode.TryGetValue(answer.QuestionCode, out var question))
                throw new InvalidOperationException($"Answer key '{answer.QuestionCode}' has no matching question.");
            if (!question.Options.Any(option => option.Key == answer.CorrectOption))
                throw new InvalidOperationException($"Answer key '{answer.QuestionCode}' references an unknown option.");
        }

        if (!questionByCode.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(answerByCode.Keys))
            throw new InvalidOperationException("Student questions and answer key must contain the same question codes.");

        return new AdaptiveQuizSnapshot(
            result.QuizVersion,
            result.QuizTitle,
            result.GenerationReason,
            result.TargetSkills ?? [],
            result.StudentQuestions,
            result.AnswerKey,
            "ai");
    }

    public AdaptiveQuizStudentResponse CreateStudentResponse(AdaptiveQuizSnapshot quiz) =>
        new(
            quiz.QuizVersion,
            quiz.QuizTitle,
            quiz.GenerationReason,
            quiz.TargetSkills,
            quiz.StudentQuestions,
            quiz.Source);

    public AdaptiveQuizScoreResult Score(AdaptiveQuizSnapshot quiz, IReadOnlyList<int> selectedIndexes)
    {
        if (selectedIndexes.Count != quiz.StudentQuestions.Count)
            throw new ArgumentException("Answer count must match the generated quiz.", nameof(selectedIndexes));

        var answerByCode = quiz.AnswerKey.ToDictionary(answer => answer.QuestionCode, StringComparer.Ordinal);
        var canonical = new List<QuizQuestion>(quiz.StudentQuestions.Count);
        var score = 0;

        for (var index = 0; index < quiz.StudentQuestions.Count; index++)
        {
            var question = quiz.StudentQuestions[index];
            var selectedIndex = selectedIndexes[index];
            if (selectedIndex < 0 || selectedIndex >= question.Options.Count)
                throw new ArgumentException($"Answer {index} is outside the option range.", nameof(selectedIndexes));

            if (!answerByCode.TryGetValue(question.QuestionCode, out var answer))
                throw new InvalidOperationException($"Missing answer key for '{question.QuestionCode}'.");

            var correctIndex = question.Options.ToList().FindIndex(option => option.Key == answer.CorrectOption);
            if (correctIndex < 0)
                throw new InvalidOperationException($"Correct option for '{question.QuestionCode}' does not exist.");
            if (selectedIndex == correctIndex)
                score++;

            canonical.Add(new QuizQuestion(
                question.Prompt,
                question.Options.Select(option => option.Text).ToList(),
                correctIndex,
                answer.Explanation)
            {
                QuestionCode = question.QuestionCode,
                SkillTag = question.SkillTag,
                Difficulty = question.Difficulty,
                ScenarioContext = question.ScenarioContext
            });
        }

        return new AdaptiveQuizScoreResult(score, quiz.StudentQuestions.Count, canonical);
    }


    public AdaptiveQuizSnapshot CreateStaticFallback(string actorType)
    {
        var catalog = QuizCatalog.ForActor(actorType);
        var questions = catalog.Select((question, index) => new AdaptiveQuizStudentQuestion(
            $"STATIC-{actorType.ToUpperInvariant()}-{index + 1}",
            question.Prompt,
            question.Options.Select((text, optionIndex) => new AdaptiveQuizOption(
                ((char)('A' + optionIndex)).ToString(),
                text)).ToList(),
            question.SkillTag ?? "general-communication",
            question.Difficulty ?? "easy",
            question.ScenarioContext ?? actorType,
            false)).ToList();
        var answers = catalog.Select((question, index) => new AdaptiveQuizAnswerKey(
            questions[index].QuestionCode,
            questions[index].Options[question.CorrectIndex].Key,
            question.Explanation,
            ["static-catalog"])).ToList();

        return new AdaptiveQuizSnapshot(
            "static-v1",
            "Communication check",
            "Static fallback used because an adaptive quiz was unavailable.",
            questions.Select(question => question.SkillTag).Distinct(StringComparer.Ordinal).ToList(),
            questions,
            answers,
            "static-fallback");
    }
}
