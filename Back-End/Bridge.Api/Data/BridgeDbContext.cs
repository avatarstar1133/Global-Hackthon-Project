using Bridge.Api.Domain;
using Microsoft.EntityFrameworkCore;
namespace Bridge.Api.Data;

public sealed class BridgeDbContext(DbContextOptions<BridgeDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>(); public DbSet<UserProfile> UserProfiles => Set<UserProfile>(); public DbSet<Actor> Actors => Set<Actor>(); public DbSet<Scenario> Scenarios => Set<Scenario>(); public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>(); public DbSet<ChatMessage> Messages => Set<ChatMessage>(); public DbSet<SessionEvaluation> SessionEvaluations => Set<SessionEvaluation>(); public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>().HasOne(x => x.Profile).WithOne(x => x.User).HasForeignKey<UserProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade); b.Entity<UserProfile>().HasKey(x => x.UserId);
        b.Entity<Actor>().HasIndex(x => x.ActorType); b.Entity<Scenario>().HasOne(x => x.Actor).WithMany(x => x.Scenarios).HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PracticeSession>().HasIndex(x => new { x.UserId, x.StartedAt }); b.Entity<PracticeSession>().HasOne(x => x.User).WithMany(x => x.Sessions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade); b.Entity<PracticeSession>().HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict); b.Entity<PracticeSession>().HasOne(x => x.Scenario).WithMany().HasForeignKey(x => x.ScenarioId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ChatMessage>().HasIndex(x => new { x.SessionId, x.SequenceNumber }).IsUnique(); b.Entity<ChatMessage>().HasIndex(x => new { x.SessionId, x.ClientMessageId }).IsUnique().HasFilter("[ClientMessageId] IS NOT NULL"); b.Entity<ChatMessage>().Property(x => x.Content).HasMaxLength(4000); b.Entity<ChatMessage>().HasOne(x => x.Session).WithMany(x => x.Messages).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<SessionEvaluation>().HasIndex(x => x.SessionId).IsUnique(); b.Entity<SessionEvaluation>().HasOne(x => x.Session).WithOne(x => x.Evaluation).HasForeignKey<SessionEvaluation>(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<QuizAttempt>().HasOne(x => x.Session).WithMany(x => x.QuizAttempts).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade); b.Entity<QuizAttempt>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
    }
}
