using Bridge.Api.Domain;
using Microsoft.EntityFrameworkCore;
namespace Bridge.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(BridgeDbContext db, CancellationToken ct = default)
    {
        var colleague = await db.Actors.Include(x => x.Scenarios).SingleOrDefaultAsync(x => x.ActorType == "colleague", ct);
        if (colleague is not null)
        {
            colleague.IsActive = false;
            foreach (var scenario in colleague.Scenarios) scenario.IsActive = false;
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
        await db.SaveChangesAsync(ct);
    }
    private static Scenario S(string id, string actor, string title, string description, string goal, string difficulty, string opening, string culture, string instructions) => new() { Id = id, ActorId = actor, Title = title, Description = description, Goal = goal, Difficulty = difficulty, OpeningMessage = opening, CultureContext = culture, PromptInstructions = instructions };
}
