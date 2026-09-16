using Bridge.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Data;

public static class DatabaseSeeder
{
    private const string PersonaAssessmentId = "persona-v1";

    public static async Task SeedAsync(BridgeDbContext db, CancellationToken ct = default)
    {
        var colleague = await db.Actors.Include(x => x.Scenarios).SingleOrDefaultAsync(x => x.ActorType == "colleague", ct);
        if (colleague is not null)
        {
            colleague.IsActive = false;
            foreach (var scenario in colleague.Scenarios)
                scenario.IsActive = false;
        }

        await db.UserProfiles.Where(x => x.HardestActorType == "colleague")
            .ExecuteUpdateAsync(update => update.SetProperty(x => x.HardestActorType, "friend"), ct);

        if (!await db.Actors.AnyAsync(x => x.ActorType == "friend", ct))
        {
            db.Actors.Add(new Actor { Id = "friend-jake", ActorType = "friend", Name = "Jake", Role = "American classmate", Personality = "Casual and friendly.", CommunicationStyle = "Natural conversation." });
            db.Scenarios.AddRange(
                S("friend-class", "friend-jake", "Start a conversation in class", "Meet a classmate.", "Introduce yourself and ask a follow-up.", "easy", "Hey! Is this seat taken?", "Peers bond through reciprocal sharing.", "Keep it casual."),
                S("friend-party", "friend-jake", "Small talk at a party", "Talk to someone new.", "Continue for several turns.", "easy", "How do you know everyone here?", "Small talk finds shared interests.", "Use friendly curiosity."),
                S("friend-disagree", "friend-jake", "Disagree with a friend", "Share a different opinion.", "State a view and preserve warmth.", "medium", "Group projects are always better. Don't you?", "Light disagreement can signal engagement.", "Let the learner disagree."));
        }

        if (!await db.Actors.AnyAsync(x => x.ActorType == "professor", ct))
        {
            db.Actors.Add(new Actor { Id = "professor-dr-miller", ActorType = "professor", Name = "Dr. Miller", Role = "American professor", Personality = "Professional and approachable.", CommunicationStyle = "Clear and concise." });
            db.Scenarios.AddRange(
                S("professor-office-hours", "professor-dr-miller", "Introduce yourself in office hours", "Visit a professor.", "Identify yourself, class and question.", "medium", "Hi, what can I help with?", "Professors expect office-hour visits.", "Ask for purpose if unclear."),
                S("professor-extension", "professor-dr-miller", "Ask for a deadline extension", "Request two more days.", "Make a clear request, reason and date.", "hard", "What would you like to discuss?", "Specific requests are clearer than apologies.", "Require a specific request."),
                S("professor-feedback", "professor-dr-miller", "Respond to critical feedback", "Discuss a criticized draft.", "Acknowledge feedback and ask what to improve.", "hard", "Your argument is not clear enough yet.", "Questions show initiative.", "Be firm and constructive."));
        }

        if (!await db.AssessmentDefinitions.AnyAsync(x => x.Id == PersonaAssessmentId, ct))
            db.AssessmentDefinitions.Add(BuildPersonaAssessment());

        await db.SaveChangesAsync(ct);
    }

    private static AssessmentDefinition BuildPersonaAssessment()
    {
        var assessment = new AssessmentDefinition { Id = PersonaAssessmentId, Name = "Persona Assessment", Version = "v1" };
        assessment.Sections.AddRange(
            Section("context", "Context", 1,
                Choice("Q1", 1, "What is your current academic status in the United States of America?",
                    ("pre-arrival-first-year", "Pre-arrival / first year"),
                    ("undergraduate-upperclassman", "Undergraduate, upperclassman"),
                    ("graduate-phd", "Graduate/PhD student")),
                Scale("Q2", 2, "How confident do you feel speaking English in an academic setting (class, presentations, office hours)?", "Very anxious", "Very confident")),
            Section("communication-context", "Communication Behavior by Context", 2,
                Scale("Q3", 3, "In a small class discussion, I speak up to share my opinion without being called on.", "Never", "Very often"),
                Scale("Q4", 4, "When I disagree with a professor's statement in class, I say so openly during the discussion.", "Never", "Very often"),
                Scale("Q5", 5, "I ask a professor for help via email more comfortably than in person or in class.", "Never", "Very often")),
            Section("situational-judgment", "Situational Judgment Scenarios", 3,
                Choice("Q6", 6, "Your professor makes a factual mistake while lecturing that you're fairly sure about. What would you most likely do?",
                    ("say-nothing", "Say nothing"),
                    ("mention-privately", "Wait and mention it privately after class"),
                    ("clarifying-question", "Ask a clarifying question that gently points to the issue"),
                    ("correct-directly", "Raise my hand and correct it directly in class")),
                Choice("Q7", 7, "You strongly disagree with a classmate's argument during a seminar. You would:",
                    ("stay-quiet", "Stay quiet to avoid conflict"),
                    ("softly-add", "Agree partially, then softly add \"but have you considered...\""),
                    ("state-disagreement", "Directly state \"I disagree, and here's why\""),
                    ("talk-privately", "Talk to them privately afterward instead")),
                Choice("Q8", 8, "You need a deadline extension. You would:",
                    ("do-not-ask", "Not ask, and submit late/weaker work"),
                    ("ask-peer-first", "Ask a friend/senior student first if it's okay to ask"),
                    ("email-professor", "Email the professor directly explaining the reason"),
                    ("ask-in-person", "Ask in person, briefly and apologetically")),
                Choice("Q9", 9, "During office hours, your professor asks \"Any questions?\" You have one, but it might expose you didn't fully understand. You would:",
                    ("say-no", "Say \"No, I'm good\""),
                    ("safer-question", "Ask a related but \"safer\" question instead"),
                    ("ask-directly", "Ask exactly what you're confused about, directly"))),
            Section("anxiety-time-retention", "Anxiety, Time & Retention", 4,
                MultiChoice("Q10", 10, "What worries you most when communicating in an American classroom?", 2,
                    ("losing-face", "Being wrong in front of others / losing face"),
                    ("english-words", "Not finding the right English words fast enough"),
                    ("professor-appropriateness", "Not knowing what's \"appropriate\" to say to a professor"),
                    ("too-direct", "Being seen as too direct/rude"),
                    ("too-passive", "Being seen as passive / having nothing to contribute")),
                Choice("Q11", 11, "What's your main goal right now?",
                    ("speak-up-confidence", "Feel less anxious speaking up in class"),
                    ("disagree-feedback", "Learn how to disagree/give feedback appropriately"),
                    ("professor-communication", "Improve professor/advisor communication (emails, office hours)"),
                    ("group-project", "Improve group project communication"),
                    ("general-fluency", "General fluency and confidence")),
                Choice("Q12", 12, "How much time can you realistically spend practicing per day?",
                    ("under-5", "Less than 5 minutes"),
                    ("5-10", "5-10 minutes"),
                    ("10-20", "10-20 minutes"),
                    ("20-plus", "20+ minutes")),
                MultiChoice("Q13", 13, "What would make you come back and continue tomorrow?", 2,
                    ("reminder", "A quick reminder/notification at a time that works for me"),
                    ("visible-progress", "Seeing visible progress (streaks, scores, \"you improved X%\")"),
                    ("new-scenario", "A new, specific scenario each day (not repeating the same thing)"),
                    ("real-life", "Feeling like the practice actually reflects my real life (class, email, professor)"),
                    ("low-pressure", "A short, low-pressure format - no fear of \"failing\""))));
        return assessment;
    }

    private static AssessmentSection Section(string key, string title, int order, params AssessmentQuestion[] questions)
    {
        var id = PersonaAssessmentId + "-" + key;
        foreach (var question in questions)
            question.SectionId = id;
        return new AssessmentSection { Id = id, AssessmentDefinitionId = PersonaAssessmentId, Title = title, Order = order, Questions = [.. questions] };
    }

    private static AssessmentQuestion Scale(string code, int order, string prompt, string minLabel, string maxLabel) =>
        new() { Id = QuestionId(code), Code = code, Prompt = prompt, AnswerType = "scale", Order = order, ScaleMin = 1, ScaleMax = 5, ScaleMinLabel = minLabel, ScaleMaxLabel = maxLabel };

    private static AssessmentQuestion Choice(string code, int order, string prompt, params (string Value, string Label)[] options) =>
        ChoiceQuestion(code, order, prompt, "single-choice", 1, options);

    private static AssessmentQuestion MultiChoice(string code, int order, string prompt, int maxSelections, params (string Value, string Label)[] options) =>
        ChoiceQuestion(code, order, prompt, "multi-select", maxSelections, options);

    private static AssessmentQuestion ChoiceQuestion(string code, int order, string prompt, string answerType, int maxSelections, (string Value, string Label)[] options)
    {
        var questionId = QuestionId(code);
        var question = new AssessmentQuestion { Id = questionId, Code = code, Prompt = prompt, AnswerType = answerType, Order = order, MaxSelections = maxSelections };
        question.Options.AddRange(options.Select((option, index) => new AssessmentOption
        {
            Id = questionId + "-" + option.Value,
            QuestionId = questionId,
            Value = option.Value,
            Label = option.Label,
            Order = index + 1
        }));
        return question;
    }

    private static string QuestionId(string code) => PersonaAssessmentId + "-" + code.ToLowerInvariant();

    private static Scenario S(string id, string actor, string title, string description, string goal, string difficulty, string opening, string culture, string instructions) =>
        new() { Id = id, ActorId = actor, Title = title, Description = description, Goal = goal, Difficulty = difficulty, OpeningMessage = opening, CultureContext = culture, PromptInstructions = instructions };
}
