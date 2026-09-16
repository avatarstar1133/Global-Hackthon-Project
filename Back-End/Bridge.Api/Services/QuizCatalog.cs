using Bridge.Api.Contracts;
namespace Bridge.Api.Services;

public static class QuizCatalog
{
    public static IReadOnlyList<QuizQuestion> ForActor(string type) => type switch
    {
        "professor" => [new("Which request is clearest?", ["Maybe more time?", "Could I submit Friday instead of Wednesday?", "I cannot do it."], 1, "Use a specific request and date."), new("Best response to feedback?", ["Stay silent.", "Argue.", "Ask what to revise first."], 2, "Focused questions show initiative."), new("Begin office hours how?", ["Apologize.", "Name yourself, class and question.", "Wait."], 1, "Brief context helps.")],
        _ => [new("Continue small talk?", ["Ask a follow-up.", "One word.", "Leave."], 0, "Reciprocal questions connect."), new("Disagree warmly?", ["Wrong.", "I see it differently because...", "Pretend."], 1, "Use a warm opener."), new("After introducing yourself?", ["Ask or share a detail.", "Silence.", "Apologize."], 0, "Balance sharing and asking.")]
    };
}
