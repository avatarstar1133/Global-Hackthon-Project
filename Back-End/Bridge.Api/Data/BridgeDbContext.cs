using Bridge.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Data;

public sealed class BridgeDbContext(DbContextOptions<BridgeDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();
    public DbSet<SessionEvaluation> SessionEvaluations => Set<SessionEvaluation>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<AssessmentDefinition> AssessmentDefinitions => Set<AssessmentDefinition>();
    public DbSet<AssessmentSection> AssessmentSections => Set<AssessmentSection>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<AssessmentOption> AssessmentOptions => Set<AssessmentOption>();
    public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
    public DbSet<AssessmentResponse> AssessmentResponses => Set<AssessmentResponse>();
    public DbSet<PersonaAnalysis> PersonaAnalyses => Set<PersonaAnalysis>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>().HasOne(x => x.Profile).WithOne(x => x.User).HasForeignKey<UserProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<UserProfile>().HasKey(x => x.UserId);
        b.Entity<Actor>().HasIndex(x => x.ActorType);
        b.Entity<Scenario>().HasOne(x => x.Actor).WithMany(x => x.Scenarios).HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PracticeSession>().HasIndex(x => new { x.UserId, x.StartedAt });
        b.Entity<PracticeSession>().HasOne(x => x.User).WithMany(x => x.Sessions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<PracticeSession>().HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PracticeSession>().HasOne(x => x.Scenario).WithMany().HasForeignKey(x => x.ScenarioId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PracticeSession>().Property(x => x.QuizGenerationModel).HasMaxLength(100);
        b.Entity<PracticeSession>().Property(x => x.QuizGenerationPromptVersion).HasMaxLength(50);
        b.Entity<ChatMessage>().HasIndex(x => new { x.SessionId, x.SequenceNumber }).IsUnique();
        b.Entity<ChatMessage>().HasIndex(x => new { x.SessionId, x.ClientMessageId }).IsUnique().HasFilter("[ClientMessageId] IS NOT NULL");
        b.Entity<ChatMessage>().Property(x => x.Content).HasMaxLength(4000);
        b.Entity<ChatMessage>().HasOne(x => x.Session).WithMany(x => x.Messages).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<SessionEvaluation>().HasIndex(x => x.SessionId).IsUnique();
        b.Entity<SessionEvaluation>().HasOne(x => x.Session).WithOne(x => x.Evaluation).HasForeignKey<SessionEvaluation>(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<QuizAttempt>().HasOne(x => x.Session).WithMany(x => x.QuizAttempts).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<QuizAttempt>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<QuizAttempt>().Property(x => x.EvaluationModel).HasMaxLength(100);
        b.Entity<QuizAttempt>().Property(x => x.EvaluationPromptVersion).HasMaxLength(50);

        b.Entity<AssessmentDefinition>().Property(x => x.Id).HasMaxLength(100);
        b.Entity<AssessmentDefinition>().Property(x => x.Name).HasMaxLength(200);
        b.Entity<AssessmentDefinition>().Property(x => x.Version).HasMaxLength(32);
        b.Entity<AssessmentDefinition>().HasIndex(x => new { x.Name, x.Version }).IsUnique();

        b.Entity<AssessmentSection>().Property(x => x.Id).HasMaxLength(150);
        b.Entity<AssessmentSection>().Property(x => x.AssessmentDefinitionId).HasMaxLength(100);
        b.Entity<AssessmentSection>().Property(x => x.Title).HasMaxLength(200);
        b.Entity<AssessmentSection>().Property(x => x.Description).HasMaxLength(500);
        b.Entity<AssessmentSection>().HasIndex(x => new { x.AssessmentDefinitionId, x.Order }).IsUnique();
        b.Entity<AssessmentSection>().HasOne(x => x.AssessmentDefinition).WithMany(x => x.Sections).HasForeignKey(x => x.AssessmentDefinitionId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<AssessmentQuestion>().Property(x => x.Id).HasMaxLength(150);
        b.Entity<AssessmentQuestion>().Property(x => x.SectionId).HasMaxLength(150);
        b.Entity<AssessmentQuestion>().Property(x => x.Code).HasMaxLength(20);
        b.Entity<AssessmentQuestion>().Property(x => x.Prompt).HasMaxLength(1000);
        b.Entity<AssessmentQuestion>().Property(x => x.AnswerType).HasMaxLength(32);
        b.Entity<AssessmentQuestion>().Property(x => x.ScaleMinLabel).HasMaxLength(100);
        b.Entity<AssessmentQuestion>().Property(x => x.ScaleMaxLabel).HasMaxLength(100);
        b.Entity<AssessmentQuestion>().HasIndex(x => new { x.SectionId, x.Order }).IsUnique();
        b.Entity<AssessmentQuestion>().HasOne(x => x.Section).WithMany(x => x.Questions).HasForeignKey(x => x.SectionId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<AssessmentOption>().Property(x => x.Id).HasMaxLength(180);
        b.Entity<AssessmentOption>().Property(x => x.QuestionId).HasMaxLength(150);
        b.Entity<AssessmentOption>().Property(x => x.Value).HasMaxLength(100);
        b.Entity<AssessmentOption>().Property(x => x.Label).HasMaxLength(500);
        b.Entity<AssessmentOption>().HasIndex(x => new { x.QuestionId, x.Order }).IsUnique();
        b.Entity<AssessmentOption>().HasIndex(x => new { x.QuestionId, x.Value }).IsUnique();
        b.Entity<AssessmentOption>().HasOne(x => x.Question).WithMany(x => x.Options).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<AssessmentAttempt>().Property(x => x.AssessmentId).HasMaxLength(100);
        b.Entity<AssessmentAttempt>().Property(x => x.AssessmentVersion).HasMaxLength(32);
        b.Entity<AssessmentAttempt>().Property(x => x.AnalysisStatus).HasMaxLength(20);
        b.Entity<AssessmentAttempt>().HasIndex(x => new { x.UserId, x.CreatedAt });
        b.Entity<AssessmentAttempt>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<AssessmentAttempt>().HasOne(x => x.Assessment).WithMany().HasForeignKey(x => x.AssessmentId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<AssessmentResponse>().Property(x => x.QuestionId).HasMaxLength(150);
        b.Entity<AssessmentResponse>().Property(x => x.QuestionCode).HasMaxLength(20);
        b.Entity<AssessmentResponse>().Property(x => x.AnswerType).HasMaxLength(32);
        b.Entity<AssessmentResponse>().Property(x => x.SelectedValuesJson).HasMaxLength(500);
        b.Entity<AssessmentResponse>().HasIndex(x => new { x.AttemptId, x.QuestionId }).IsUnique();
        b.Entity<AssessmentResponse>().HasOne(x => x.Attempt).WithMany(x => x.Responses).HasForeignKey(x => x.AttemptId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<AssessmentResponse>().HasOne(x => x.Question).WithMany().HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<PersonaAnalysis>().Property(x => x.PromptVersion).HasMaxLength(50);
        b.Entity<PersonaAnalysis>().Property(x => x.ModelName).HasMaxLength(100);
        b.Entity<PersonaAnalysis>().Property(x => x.Summary).HasMaxLength(2000);
        b.Entity<PersonaAnalysis>().HasIndex(x => x.UserId);
        b.Entity<PersonaAnalysis>().HasOne(x => x.Attempt).WithOne(x => x.Analysis).HasForeignKey<PersonaAnalysis>(x => x.AttemptId).OnDelete(DeleteBehavior.Cascade);
    }
}
