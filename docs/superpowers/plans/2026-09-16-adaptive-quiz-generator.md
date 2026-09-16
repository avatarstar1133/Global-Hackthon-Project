# Adaptive Quiz Generator Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generate and securely persist a personalized post-chat quiz using the supplied Lanco system prompt.

**Architecture:** A pure AdaptiveQuizService owns AI JSON validation, safe student projection, fallback conversion, and scoring. Minimal API endpoints orchestrate EF Core and IAiClient, while PracticeSession stores an idempotent private quiz snapshot.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, EF Core 10, SQL Server, xUnit, System.Text.Json.

---

### Task 1: Lock the quiz contract with failing tests

**Files:**
- Create: `Back-End/Bridge.Api.Tests/AdaptiveQuizServiceTests.cs`
- Create: `Back-End/Bridge.Api/Services/AdaptiveQuizService.cs`
- Modify: `Back-End/Bridge.Api/Contracts/ApiContracts.cs`

- [ ] Add tests for valid prompt JSON, mismatched answer codes, invalid correct keys, safe student projection, and scoring.
- [ ] Run `dotnet test Bridge.Api.Tests --filter AdaptiveQuizServiceTests` and confirm failure because the service/contracts are missing.
- [ ] Implement the smallest contract and service that makes the tests pass.
- [ ] Re-run the filtered tests.

### Task 2: Install the supplied prompt and build rich AI input

**Files:**
- Create: `Back-End/Bridge.Api/Prompts/quiz-generation-v1.txt`
- Modify: `Back-End/Bridge.Api/Services/QuizGeneration.cs`
- Modify: `Back-End/Bridge.Api/Program.cs`

- [ ] Copy the supplied prompt into the project.
- [ ] Extend the service to build JSON input containing persona_profile, chat_transcript, session_evaluation, quiz_question_bank, previously_seen_question_codes, and requested_question_count=5.
- [ ] Register AdaptiveQuizService.
- [ ] Test input construction without an AI mock.

### Task 3: Persist and serve the private quiz snapshot

**Files:**
- Modify: `Back-End/Bridge.Api/Domain/Entities.cs`
- Modify: `Back-End/Bridge.Api/Data/BridgeDbContext.cs`
- Modify: `Back-End/Bridge.Api/Api/BridgeEndpoints.cs`
- Create: `Back-End/Bridge.Api/Data/Migrations/<timestamp>_AddAdaptiveQuizPersistence.cs`

- [ ] Require SessionEvaluation before generation.
- [ ] Return an existing stored quiz without calling AI again.
- [ ] Parse and validate AI output, or store a labeled static fallback.
- [ ] Return only AdaptiveQuizStudentResponse.
- [ ] Store model, prompt version, and generated timestamp.
- [ ] Create and apply the EF migration.

### Task 4: Score the stored dynamic quiz

**Files:**
- Modify: `Back-End/Bridge.Api/Api/BridgeEndpoints.cs`
- Modify: `Back-End/Bridge.Api/Contracts/ApiContracts.cs`

- [ ] Read AdaptiveQuizSnapshot during submit.
- [ ] Validate answer count and indexes.
- [ ] Score using answer-key option keys.
- [ ] Persist canonical QuizQuestion data for PostQuiz Evaluator compatibility.
- [ ] Keep the existing submit response shape.

### Task 5: Verify

- [ ] Run `dotnet restore`.
- [ ] Run `dotnet format --verify-no-changes`.
- [ ] Run `dotnet build --nologo`.
- [ ] Run `dotnet test --nologo --no-build`.
- [ ] Run `dotnet ef migrations has-pending-model-changes`.
- [ ] Call generation and submit endpoints against localhost and confirm no answer key appears in generation JSON.
- [ ] Review `git diff --check` and do not commit or push.
