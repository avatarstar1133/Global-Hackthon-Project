<img width="1817" height="202" alt="image" src="https://github.com/user-attachments/assets/3db34d92-91d9-43a9-82dc-b8e5ad5e9f31" />Lanco

Practice the hard conversations before they happen.

Lanco is an AI-powered intercultural communication practice platform designed for international students who may feel overwhelmed by language, classroom expectations, and cultural differences when studying abroad.

Instead of teaching English only as grammar or vocabulary, Lanco focuses on real academic and social communication situations such as talking with classmates, asking a professor for clarification, disagreeing respectfully, requesting help, participating in discussions, and navigating unfamiliar university communication norms.

The system combines persona assessment, AI role-play, session review, adaptive quizzes, and progress tracking to create a personalized practice loop.

1. Problem

International students may understand English academically but still struggle with questions such as:

How direct should I be with a professor?

Is it acceptable to disagree in class?

How do I continue small talk naturally?

How do I ask for help without sounding demanding?

How do I express disagreement without becoming too passive or too blunt?

What communication style is expected in group projects, office hours, or classroom discussions?

These difficulties are not only language problems. They can involve:

language confidence;

pragmatic communication;

authority relationships;

directness and politeness;

public versus private communication;

cultural expectations;

help-seeking behavior;

peer communication and belonging.

Lanco provides a safe environment where users can practice these situations repeatedly before facing them in real life.

2. Main Learning Flow

User enters Lanco
      ↓
Persona Assessment
      ↓
Personalized Communication Profile
      ↓
Choose / Receive Practice Scenario
      ↓
AI Role-play Conversation
      ↓
Done
      ↓
Session Review
      ↓
Adaptive Quiz
      ↓
Post-Quiz Evaluation
      ↓
Progress / Next Practice Cycle

The main principle is that each learning step should use evidence from the user's actual behavior rather than assigning fixed traits based on nationality or cultural background.

3. Core Features

Persona Assessment

When a user first enters the system, Lanco collects onboarding information about areas such as:

academic situation;

confidence communicating in academic English;

classroom participation;

comfort disagreeing with professors;

private versus public communication;

help-seeking behavior;

communication anxiety;

primary learning goal;

available practice time.

The backend can use an AI persona-analysis prompt to create a structured communication profile.

The assessment is not a pass/fail cultural test. It is used to personalize later practice.

AI Role-play Practice

Users practice conversations with university-related actors such as:

American classmates;

professors;

other supported academic roles.

Each scenario contains:

a communication goal;

difficulty;

cultural context;

opening message;

actor personality and communication style.

Examples include:

introducing yourself to a classmate;

continuing small talk;

asking a professor for clarification;

office-hour conversations;

respectful disagreement;

requesting academic support.

The AI continues the conversation based on the selected actor and scenario.

Hint System

During a practice conversation, users can request a hint.

Hints are generated from the current scenario and conversation context rather than exposing a full scripted answer.

Session Review

When the user presses Done, the complete conversation is evaluated.

The current evaluation model includes dimensions such as:

overall communication;

clarity;

directness;

warmth;

engagement;

goal completion.

The review can also provide:

strengths;

improvement areas;

cultural communication notes;

a summary of the session.

Adaptive Quiz

After the conversation review, Lanco generates a quiz based on:

the user's persona profile;

the recent chat transcript;

the scenario;

the session evaluation;

previous quiz questions;

the available question bank.

The quiz generator is designed to emphasize practical communication judgment rather than memorization.

Typical question areas include:

assertiveness;

responsiveness;

professor communication;

peer communication;

help-seeking;

disagreement;

public versus private communication;

appropriate directness;

cultural-pragmatic choices.

The backend keeps the answer key server-side. The frontend receives only the student-facing questions before submission.

If AI quiz generation fails, the application can use a deterministic fallback quiz.

Current backend configuration requests 5 questions per adaptive quiz.

Post-Quiz Evaluation

After quiz submission, Lanco can evaluate:

total performance;

stronger communication skills;

weaker communication skills;

patterns across skill tags;

suggested next learning targets.

Quiz performance should not be treated as a complete measure of communication ability. It is combined with persona and conversation evidence.

Progress Tracking

The application stores sessions, evaluations, and quiz attempts in SQL Server.

This allows the system to track learning history instead of treating every visit as a completely new user.

4. Research-Grounded Design

Lanco's communication framework is informed by research related to:

advisor-advisee communication;

assertiveness and responsiveness;

instructor immediacy and positive Instructor Talk;

context-sensitive communication behavior;

within-culture individual variation;

harmony, face, authority, reciprocity, and respect;

peer communication;

school connectedness;

academic self-efficacy.

The research is used as a knowledge framework, not as a way to stereotype users.

Key rule:

The user's actual answers and behavior take priority over population-level cultural patterns.

5. Technology Stack

Frontend

React 18

Vite 5

JavaScript

CSS

Backend

ASP.NET Core

.NET 10

Entity Framework Core 10

SQL Server

Swagger / OpenAPI

AI

OpenAI Responses API

Configurable OpenAI model through application settings or .env

Database

Microsoft SQL Server

EF Core migrations

Automatic migration and reference-data seeding during local startup

6. Project Structure

<img width="197" height="433" alt="image" src="https://github.com/user-attachments/assets/41cfb508-0617-4795-8570-d504d30d57b8" />


Running the Project Locally

7. Prerequisites

Install the following before running the project:

Required

.NET 10 SDK

Node.js 18+

npm

Microsoft SQL Server

SQL Server Management Studio or another SQL client, recommended

OpenAI API key

Check installations:

dotnet --version
node --version
npm --version

8. Configure SQL Server

The current development configuration uses:

"BridgeDb": "Server=HAFO;Database=BridgeDb;Trusted_Connection=True;TrustServerCertificate=True"

This means SQL Server is expected to be available under:

HAFO

with Windows Authentication.

If your SQL Server has a different server or instance name, edit:

Back-End/Bridge.Api/appsettings.json

Example for SQL Express:

"ConnectionStrings": {
  "BridgeDb": "Server=.\\SQLEXPRESS;Database=BridgeDb;Trusted_Connection=True;TrustServerCertificate=True"
}

Example for LocalDB:

"ConnectionStrings": {
  "BridgeDb": "Server=(localdb)\\MSSQLLocalDB;Database=BridgeDb;Trusted_Connection=True;TrustServerCertificate=True"
}

Do not change Server=HAFO if HAFO is already the correct SQL Server instance on your machine.

The backend runs EF Core migrations automatically on startup:

await db.Database.MigrateAsync();

Therefore BridgeDb and its migration-managed tables are prepared when the backend starts, provided the SQL Server connection is valid and the account has sufficient permissions.

9. Configure the OpenAI API Key

Go to:

Back-End/Bridge.Api/

Create:

.env

Example:

OpenAI__ApiKey=YOUR_OPENAI_API_KEY
OpenAI__Model=YOUR_ACCESSIBLE_MODEL_ID
OpenAI__BaseUrl=https://api.openai.com/v1/

The project includes a local .env loader in Program.cs.

ASP.NET configuration maps:

OpenAI__ApiKey

to:

OpenAI:ApiKey

Model configuration

appsettings.json currently contains:

"OpenAI": {
  "ApiKey": "",
  "Model": "gpt-5.6-luna",
  "BaseUrl": "https://api.openai.com/v1/"
}

A value in .env overrides the configured model.

Use a model ID that is actually available to your OpenAI API project.

Do not assume access to a model only because it is available in the ChatGPT application.

10. Security

Never commit your real API key.

The repository .gitignore already excludes:

.env
.env.*

Do not place the real secret in:

frontend source code;

App.jsx;

api.js;

GitHub;

screenshots;

README files;

public ZIP archives.

If an API key has already been exposed publicly or shared in a screenshot/chat, revoke it and create a new one.

Start the Application

11. Run the Backend

Open PowerShell:

cd Global-Hackthon-Project-main\Back-End\Bridge.Api

Restore dependencies:

dotnet restore

Build:

dotnet build

Start:

dotnet run

Expected output should include:

Now listening on: http://localhost:5081

Backend URL:

http://localhost:5081

Health check:

http://localhost:5081/health

Expected response:

{
  "status": "ok"
}

Swagger:

http://localhost:5081/swagger

12. Run the Frontend

Open a second PowerShell terminal:

cd Global-Hackthon-Project-main\Front-End

Install dependencies:

npm install

Start Vite:

npm run dev

Expected:

Local: http://127.0.0.1:5173/

Open:

http://127.0.0.1:5173/

13. Frontend-to-Backend Connection

Vite proxies API requests from:

/api

to:

http://localhost:5081

Configuration:

server: {
  host: '127.0.0.1',
  port: 5173,
  proxy: {
    '/api': {
      target: 'http://localhost:5081',
      changeOrigin: true,
    },
  },
}

For local development, normally no additional frontend API configuration is required.

Main API Endpoints

14. Users and Persona

POST /api/users/anonymous
GET  /api/users/{userId}

POST /api/onboarding/assess

GET  /api/assessments/{assessmentId}
POST /api/assessments/{assessmentId}/submit

GET /api/users/{userId}/persona

15. Practice Content

GET /api/actors
GET /api/scenarios

16. Practice Session

POST /api/sessions
GET  /api/sessions/{sessionId}

POST /api/sessions/{sessionId}/messages
POST /api/sessions/{sessionId}/hint
POST /api/sessions/{sessionId}/complete

17. Learning and Quiz

GET  /api/sessions/{sessionId}/learning-plan

POST /api/sessions/{sessionId}/quiz
POST /api/sessions/{sessionId}/quiz/submit

GET /api/progress/{userId}

Troubleshooting

18. Request failed (404)

A common local-development cause is an old user ID stored in browser local storage after the database has been reset.

The frontend stores:

bridge_user_id

If necessary, open the browser DevTools console and run:

localStorage.removeItem('bridge_user_id')
location.reload()

Before debugging the frontend, verify:

http://localhost:5081/health

If /health works, the backend is running.

19. Request failed (429)

The backend has API rate limiting enabled.

Current development limit:

300 requests / minute / IP

with a small queue.

If the 429 originates from the OpenAI API instead, inspect the backend terminal. OpenAI 429 responses can represent API-side rate limits, quota, or billing/spend restrictions.

20. OpenAI Request Fails

Check these first:

.env exists under:

Back-End/Bridge.Api/.env

API key is valid.

The configured model is accessible to the API project.

Internet access is available.

Backend is restarted after changing .env.

Restart:

Ctrl+C
dotnet run

21. SQL Connection Error

Verify the configured server:

Server=HAFO

Test the connection through SSMS.

Common issues:

SQL Server service is stopped;

wrong instance name;

Windows Authentication is unavailable;

current Windows account has insufficient permissions;

BridgeDb cannot be created.

22. Frontend Opens but API Calls Fail

Verify backend:

http://localhost:5081/health

Then verify Vite is running on:

http://127.0.0.1:5173

Do not use VS Code Go Live for this React/Vite application.

Run:

npm run dev

instead.

Testing

23. Backend Tests

From:

cd Global-Hackthon-Project-main\Back-End

run:

dotnet test

The solution contains backend tests for areas including:

persona assessment;

prompt building;

adaptive quiz behavior;

JSON parsing.

24. Frontend Production Build

To verify the frontend compiles:

cd Front-End
npm run build

The generated frontend build is placed in:

Front-End/dist/

Design Principles

Lanco follows several important rules:

Behavior before stereotype
User-specific evidence is more important than cultural generalizations.

Context matters
Communication with a classmate can differ from communication with a professor.

Language and culture are related but different
A user can have strong English grammar while still feeling uncertain about pragmatic expectations.

Directness is not automatically better
Effective communication balances clarity, responsiveness, politeness, and context.

Practice should be iterative
Conversation, review, quiz, and later practice should build on previous performance.

AI output is structured
Persona analysis, quiz generation, and evaluation use structured data so the application can store and reuse the results.

Current Local URLs

Service

URL

Frontend

http://127.0.0.1:5173

Backend API

http://localhost:5081

Health

http://localhost:5081/health

Swagger

http://localhost:5081/swagger

Quick Start

# Terminal 1 - Backend
cd Global-Hackthon-Project-main\Back-End\Bridge.Api

# Create .env first:
# OpenAI__ApiKey=...
# OpenAI__Model=...
# OpenAI__BaseUrl=https://api.openai.com/v1/

dotnet restore
dotnet build
dotnet run

# Terminal 2 - Frontend
cd Global-Hackthon-Project-main\Front-End

npm install
npm run dev

Then open:

http://127.0.0.1:5173

Status

Lanco currently provides the core workflow for:

persona assessment → AI communication practice → review → adaptive quiz → post-quiz feedback → progress tracking

The architecture is designed so later iterations can extend the learning loop with richer scenario routing, larger question banks, spaced reinforcement, learning-resource recommendations, and longitudinal personalization.
