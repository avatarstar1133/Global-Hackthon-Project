namespace Bridge.Api.Services;

public sealed record LearningResource(string Title, string Source, string Url, string Why);

public static class LearningResourceCatalog
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<LearningResource>> Resources =
        new Dictionary<string, IReadOnlyList<LearningResource>>(StringComparer.OrdinalIgnoreCase)
        {
            ["clarity"] =
            [
                new("Think Fast, Talk Smart", "Stanford Graduate School of Business", "https://www.youtube.com/watch?v=HAnw168huqA", "Structure spontaneous answers clearly."),
                new("How to Speak So That People Want to Listen", "TED", "https://www.youtube.com/watch?v=eIho2S0ZahI", "Practice concise, intentional speaking.")
            ],
            ["directness"] =
            [
                new("How to Disagree Productively", "TED - Julia Dhar", "https://www.youtube.com/watch?v=phgjouv0BUA", "State a different view while preserving common ground."),
                new("Think Fast, Talk Smart", "Stanford Graduate School of Business", "https://www.youtube.com/watch?v=HAnw168huqA", "Make a request or opinion easier to understand.")
            ],
            ["warmth"] =
            [
                new("How to Speak So That People Want to Listen", "TED", "https://www.youtube.com/watch?v=eIho2S0ZahI", "Use empathy to invite connection."),
                new("10 Ways to Have a Better Conversation", "TED - Celeste Headlee", "https://www.youtube.com/watch?v=R1vskiVDwl4", "Balance honesty, brevity and listening.")
            ],
            ["engagement"] =
            [
                new("10 Ways to Have a Better Conversation", "TED - Celeste Headlee", "https://www.youtube.com/watch?v=R1vskiVDwl4", "Ask better follow-up questions."),
                new("How to Have a Good Conversation", "TEDx - Celeste Headlee", "https://www.youtube.com/watch?v=H6n3iNh4XLI", "Balance talking and listening.")
            ],
            ["goalCompletion"] =
            [
                new("Think Fast, Talk Smart", "Stanford Graduate School of Business", "https://www.youtube.com/watch?v=HAnw168huqA", "Organize the outcome before speaking."),
                new("How to Disagree Productively", "TED - Julia Dhar", "https://www.youtube.com/watch?v=phgjouv0BUA", "Move from concern to a concrete next step.")
            ]
        };

    public static IReadOnlyList<LearningResource> For(string skill) =>
        Resources.TryGetValue(skill, out var resources) ? resources : Resources["engagement"];

    public static string WeakestSkill(Domain.SessionEvaluation evaluation)
    {
        var scores = new Dictionary<string, int>
        {
            ["clarity"] = evaluation.ClarityScore,
            ["directness"] = evaluation.DirectnessScore,
            ["warmth"] = evaluation.WarmthScore,
            ["engagement"] = evaluation.EngagementScore,
            ["goalCompletion"] = evaluation.GoalCompletionScore
        };
        return scores.OrderBy(x => x.Value).First().Key;
    }
}
