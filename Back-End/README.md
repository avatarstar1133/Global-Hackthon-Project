# Bridge Back-End

ASP.NET Core 10 API for Bridge, using EF Core and SQL Server.

## What is included

- Anonymous users and onboarding communication profiles.
- Two focused American role-play actors: friend and professor.
- Nine seeded scenarios.
- Persisted sessions and complete transcripts.
- OpenAI Responses API integration with server-side secrets.
- Idempotent client message IDs.
- Done/evaluation flow based on the complete transcript.
- Context-aware quiz and progress endpoints.
- Rate limiting, CORS, request limits and centralized error responses.

## Local setup

From the Back-End directory:

~~~powershell
dotnet restore
cd Bridge.Api
dotnet user-secrets set "ConnectionStrings:BridgeDb" "Server=YOUR_SERVER;Database=BridgeDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True"
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_API_KEY"
dotnet user-secrets set "OpenAI:Model" "gpt-5-mini"
cd ..
dotnet ef database update --project Bridge.Api --startup-project Bridge.Api
dotnet run --project Bridge.Api
~~~

For Windows Authentication:

~~~powershell
dotnet user-secrets set "ConnectionStrings:BridgeDb" "Server=localhost;Database=BridgeDb;Trusted_Connection=True;TrustServerCertificate=True"
~~~

Never put the real connection string or API key in appsettings.json, .env.example, source code, screenshots or commits. User secrets are stored outside this repository.

## Main endpoints

- POST /api/users/anonymous
- POST /api/onboarding/assess
- GET /api/actors
- GET /api/scenarios?actor=professor
- POST /api/sessions
- GET /api/sessions/{id}
- POST /api/sessions/{id}/messages
- POST /api/sessions/{id}/hint
- POST /api/sessions/{id}/complete
- GET /api/sessions/{id}/learning-plan
- POST /api/sessions/{id}/quiz
- GET /api/sessions/{id}/learning-plan
- POST /api/sessions/{id}/quiz/submit
- GET /api/progress/{userId}
- GET /health

The API applies migrations and seeds reference data on startup. Production deployments should run migrations as a separate deployment step before starting multiple instances.
