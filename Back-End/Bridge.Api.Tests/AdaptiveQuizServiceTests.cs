using System.Text.Json;
using System.Text.Json.Nodes;
using Bridge.Api.Services;

namespace Bridge.Api.Tests;

public sealed class AdaptiveQuizServiceTests
{
    private readonly AdaptiveQuizService service = new();

    [Fact]
    public void ParseAndValidate_AcceptsPromptContract()
    {
        var quiz = service.ParseAndValidate(ValidJson(), 5);

        Assert.Equal("v1", quiz.QuizVersion);
        Assert.Equal(5, quiz.StudentQuestions.Count);
        Assert.Equal(5, quiz.AnswerKey.Count);
        Assert.Equal("directness", quiz.TargetSkills[0]);
    }

    [Fact]
    public void ParseAndValidate_RejectsMismatchedAnswerCodes()
    {
        var root = JsonNode.Parse(ValidJson())!;
        root["answer_key"]!.AsArray().RemoveAt(4);

        Assert.Throws<InvalidOperationException>(() =>
            service.ParseAndValidate(root.ToJsonString(), 5));
    }

    [Fact]
    public void ParseAndValidate_RejectsUnknownCorrectOption()
    {
        var root = JsonNode.Parse(ValidJson())!;
        root["answer_key"]![0]!["correct_option"] = "Z";

        Assert.Throws<InvalidOperationException>(() =>
            service.ParseAndValidate(root.ToJsonString(), 5));
    }

    [Fact]
    public void CreateStudentResponse_DoesNotExposeAnswerKey()
    {
        var quiz = service.ParseAndValidate(ValidJson(), 5);
        var response = service.CreateStudentResponse(quiz);
        var json = JsonSerializer.Serialize(response);

        Assert.DoesNotContain("correct_option", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CorrectOption", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("explanation", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("source_basis", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Score_UsesStoredOptionKeysAndBuildsCanonicalQuestions()
    {
        var quiz = service.ParseAndValidate(ValidJson(), 5);
        var result = service.Score(quiz, [1, 1, 0, 1, 1]);

        Assert.Equal(4, result.Score);
        Assert.Equal(5, result.MaxScore);
        Assert.Equal(1, result.Questions[0].CorrectIndex);
        Assert.Equal("directness", result.Questions[0].SkillTag);
        Assert.Equal("Q1", result.Questions[0].QuestionCode);
    }

    [Fact]
    public void CreateStaticFallback_ProducesScorablePrivateSnapshot()
    {
        var quiz = service.CreateStaticFallback("professor");
        var answers = Enumerable.Repeat(0, quiz.StudentQuestions.Count).ToList();

        Assert.Equal("static-fallback", quiz.Source);
        Assert.Equal(quiz.StudentQuestions.Count, quiz.AnswerKey.Count);
        Assert.NotEmpty(service.Score(quiz, answers).Questions);
    }

    private static string ValidJson()
    {
        var questions = Enumerable.Range(1, 5).Select(index => new
        {
            question_code = $"Q{index}",
            prompt = $"Question {index}",
            options = new[]
            {
                new { key = "A", text = "Avoid the issue" },
                new { key = "B", text = "State the need clearly and respectfully" },
                new { key = "C", text = "Apologize without making a request" },
                new { key = "D", text = "Respond aggressively" }
            },
            skill_tag = "directness",
            difficulty = "medium",
            scenario_context = "Professor office hours",
            is_reinforcement = false
        });
        var answers = Enumerable.Range(1, 5).Select(index => new
        {
            question_code = $"Q{index}",
            correct_option = "B",
            explanation = "B balances clarity and respect.",
            source_basis = new[] { "session_evaluation" }
        });
        return JsonSerializer.Serialize(new
        {
            quiz_version = "v1",
            quiz_title = "Practice clear requests",
            generation_reason = "The learner did not state a clear request.",
            target_skills = new[] { "directness" },
            student_questions = questions,
            answer_key = answers
        });
    }
}
