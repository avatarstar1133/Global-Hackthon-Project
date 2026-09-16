# Adaptive Quiz Generator Design

## Goal

After a completed practice session has a SessionEvaluation, generate a short quiz grounded in the learner's transcript, evaluation, latest persona analysis, and approved static question bank. Store the private answer key on the server and expose only student-safe questions.

## Flow

1. POST /api/sessions/{id}/quiz loads the completed session, transcript, evaluation, profile, latest persona analysis, prior question codes, and static question bank.
2. The backend calls Lanco Adaptive Quiz Generator with a requested count of five.
3. AdaptiveQuizService parses and validates the JSON contract: 5-15 questions, unique codes, exactly four unique A-D options for AI output, matching answer keys, valid correct options, and allowed difficulty values.
4. The server stores one AdaptiveQuizSnapshot in PracticeSession.GeneratedQuizJson with prompt/model metadata. Repeated calls return the same snapshot and do not call AI again.
5. The response contains student_questions only. It never serializes correct_option, explanation, or source_basis.
6. Quiz submit scores the selected option indexes against the stored answer key, then stores a canonical QuizQuestion list in QuizAttempt for the existing post-quiz evaluator.
7. If the prompt, API key, or AI output is unavailable, a labeled static fallback snapshot is stored and returned.

## Persistence

PracticeSession stores GeneratedQuizJson, QuizGenerationModel, QuizGenerationPromptVersion, and QuizGeneratedAt. QuizAttempt remains the record of a submitted attempt and continues to store the scored question snapshot and selected indexes.

## Compatibility

The existing POST /sessions/{id}/quiz route remains. SubmitQuizRequest continues using option indexes, so the current frontend does not need a request-shape change. Persona onboarding and post-quiz evaluation remain separate stages.

## Validation and security

Generation is allowed only after SessionEvaluation exists. AI output is rejected if question and answer codes differ, correct keys do not exist, codes repeat, option keys repeat, or counts are outside the requested range. The frontend payload type has no answer-key fields.
