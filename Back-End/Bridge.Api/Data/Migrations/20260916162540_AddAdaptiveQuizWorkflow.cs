using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdaptiveQuizWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EvaluatedAt",
                table: "QuizAttempts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvaluationJson",
                table: "QuizAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvaluationModel",
                table: "QuizAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvaluationPromptVersion",
                table: "QuizAttempts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedQuizJson",
                table: "PracticeSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "QuizGeneratedAt",
                table: "PracticeSessions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuizGenerationModel",
                table: "PracticeSessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuizGenerationPromptVersion",
                table: "PracticeSessions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvaluatedAt",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "EvaluationJson",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "EvaluationModel",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "EvaluationPromptVersion",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "GeneratedQuizJson",
                table: "PracticeSessions");

            migrationBuilder.DropColumn(
                name: "QuizGeneratedAt",
                table: "PracticeSessions");

            migrationBuilder.DropColumn(
                name: "QuizGenerationModel",
                table: "PracticeSessions");

            migrationBuilder.DropColumn(
                name: "QuizGenerationPromptVersion",
                table: "PracticeSessions");
        }
    }
}
