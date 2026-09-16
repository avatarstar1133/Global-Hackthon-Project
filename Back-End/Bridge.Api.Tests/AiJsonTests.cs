using Bridge.Api.Contracts;
using Bridge.Api.Services;

namespace Bridge.Api.Tests;

public sealed class AiJsonTests
{
    [Fact]
    public void Parse_EvaluationWithArraySummary_NormalizesSummaryToText()
    {
        var result = AiJson.Parse<EvaluationAiResult>(EvaluationJson("[\"Good start.\", \"Next, be clearer.\"]"));

        Assert.Equal("Good start. Next, be clearer.", result.Summary);
    }

    [Fact]
    public void Parse_EvaluationWithObjectSummary_NormalizesNestedText()
    {
        var result = AiJson.Parse<EvaluationAiResult>(EvaluationJson("{\"overview\":\"Good start.\",\"nextStep\":\"Next, be clearer.\"}"));

        Assert.Equal("Good start. Next, be clearer.", result.Summary);
    }

    private static string EvaluationJson(string summary) => """
        {
          "overallScore": 3,
          "clarity": 3,
          "directness": 3,
          "warmth": 4,
          "engagement": 4,
          "goalCompletion": 3,
          "strengths": [
            { "title": "Friendly opening", "evidence": "The learner greeted Jake.", "suggestion": null }
          ],
          "improvements": [
            { "title": "Be clearer", "evidence": "One answer was indirect.", "suggestion": "Use a complete sentence." }
          ],
          "cultureGap": {
            "original": "Maybe later",
            "alternative": "I cannot today, but I am free tomorrow.",
            "explanation": "The alternative states the boundary and offers a next step."
          },
          "summary":
        """ + summary + "}";
}
