using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActorType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Personality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommunicationStyle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scenarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CultureContext = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromptInstructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scenarios_Actors_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Actors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryOfOrigin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsExperience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaselineConfidence = table.Column<int>(type: "int", nullable: false),
                    CurrentConfidence = table.Column<int>(type: "int", nullable: false),
                    HardestActorType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassroomComfort = table.Column<int>(type: "int", nullable: false),
                    DisagreementComfort = table.Column<int>(type: "int", nullable: false),
                    SmallTalkComfort = table.Column<int>(type: "int", nullable: false),
                    AssessmentVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PracticeSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScenarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DirectnessLevel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromptVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeSessions_Actors_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Actors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PracticeSessions_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PracticeSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    GoalProgress = table.Column<int>(type: "int", nullable: true),
                    CultureNoteType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CultureNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_PracticeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "PracticeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    ConfidenceBefore = table.Column<int>(type: "int", nullable: false),
                    ConfidenceAfter = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_PracticeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "PracticeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    ClarityScore = table.Column<int>(type: "int", nullable: false),
                    DirectnessScore = table.Column<int>(type: "int", nullable: false),
                    WarmthScore = table.Column<int>(type: "int", nullable: false),
                    EngagementScore = table.Column<int>(type: "int", nullable: false),
                    GoalCompletionScore = table.Column<int>(type: "int", nullable: false),
                    StrengthsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImprovementsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CultureGapJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromptVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionEvaluations_PracticeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "PracticeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actors_ActorType",
                table: "Actors",
                column: "ActorType");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionId_ClientMessageId",
                table: "Messages",
                columns: new[] { "SessionId", "ClientMessageId" },
                unique: true,
                filter: "[ClientMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionId_SequenceNumber",
                table: "Messages",
                columns: new[] { "SessionId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_ActorId",
                table: "PracticeSessions",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_ScenarioId",
                table: "PracticeSessions",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_UserId_StartedAt",
                table: "PracticeSessions",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_SessionId",
                table: "QuizAttempts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_UserId",
                table: "QuizAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ActorId",
                table: "Scenarios",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionEvaluations_SessionId",
                table: "SessionEvaluations",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "SessionEvaluations");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "PracticeSessions");

            migrationBuilder.DropTable(
                name: "Scenarios");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Actors");
        }
    }
}
