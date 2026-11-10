# School Admission System

An online, first-come-first-served school admission system. A guardian applies for a seat in a class, the seat is **held** for a limited time, and it is **confirmed** by paying or **released automatically** when the time runs out. The backend guarantees that a class never gives out more seats than it has, even when many guardians apply at the same moment.

- **Backend:** ASP.NET Core Web API (.NET 10), EF Core, PostgreSQL 17
- **Frontend:** React + Vite (JavaScript)
- **Run:** Docker + docker compose

## Run it

Only Docker is required.

```bash
docker compose up --build
```

| What | URL |
|---|---|
| Frontend (open this) | http://localhost:3000 |
| Backend API | http://localhost:8080 |
| Health check | http://localhost:8080/health |

The database tables are created automatically when the backend starts. Data is stored in the named Docker volume `pgdata`, so it survives `docker compose down` / `docker compose up`. (`docker compose down -v` deletes it.)

### Using the app

1. Open **School admin** and create a class, e.g. "Class 6 – Morning Shift" with 120 seats. Each shift is a separate class.
2. Open **Classes**, enter the student's 17-digit birth registration number at the top, and click **Apply**.
3. The application page shows a countdown. Click **Pay admission fee** to confirm, or **Cancel**. If time runs out, the status changes to **EXPIRED** by itself and the seat is released.
4. **My applications** lists all applications for the current student.

## Run the tests

The backend has 8 automated tests, including 2 concurrency tests (200 parallel applications for a 10-seat class must produce exactly 10 holds).

```bash
docker compose --profile test run --rm --build backend-tests
```
Frontend tests:

```bash
docker compose --profile test run --rm --build frontend-tests
```

This starts the database if needed and runs the tests against a separate test database (`admission_test`).

Without Docker (needs the .NET 10 SDK and the database running on port 5433):

```bash
docker compose up -d db
cd backend
dotnet test
```

## Configuration

Every value has a default in `docker-compose.yml`, so no `.env` file is needed. To override values locally, copy `.env.example` to `.env`.

| Variable | Default | Purpose |
|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` | How long a seat stays HELD before it expires |
| `API_PORT` | `8080` | Host port of the backend API |
| `FRONTEND_PORT` | `3000` | Host port of the frontend |
| `API_BASE_URL` | `http://localhost:8080` | Backend URL the browser calls (passed to the frontend container) |
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | `admission` | Database name and login |
| `DB_PORT` | `5433` | Host port to reach PostgreSQL from your machine |

The backend reads its database connection from the `ConnectionStrings__Default` environment variable (set in `docker-compose.yml`).

## API

| Method | Path | Header `X-Student-Id` | Success |
|---|---|---|---|
| POST | `/classes` | – | 201 |
| GET | `/classes` | – | 200 |
| GET | `/classes/{classId}` | – | 200 |
| POST | `/classes/{classId}/holds` | required | 201 (409 if no seats) |
| GET | `/holds/{holdId}` | optional | 200 |
| POST | `/holds/{holdId}/confirm` | required | 200 |
| DELETE | `/holds/{holdId}` | required | 204 |
| GET | `/me/holds` | required | 200 |

Errors always use the same JSON shape: `{ "error": "NO_SEATS_AVAILABLE", "message": "Sorry, all seats in this class are taken right now." }`

## Project structure

```
backend/
  SchoolAdmission.Api/        Presentation tier: controllers, error middleware, background expiry job, Program.cs
  SchoolAdmission.Business/   Business tier: services (rules, locking flow), DTOs, errors, validation
  SchoolAdmission.Data/       Data tier: entities, DbContext, repositories, unit of work, migrations
  SchoolAdmission.Tests/      xUnit API tests, including concurrency tests
  Dockerfile
frontend/
  src/pages/                  Classes, Application, My applications, School admin
  src/components/             Student ID bar, error message
  src/api.js                  All backend calls
  Dockerfile
docker-compose.yml
```

## How the key rules are guaranteed

- **No overbooking:** applying runs in one database transaction that first locks the class row (`SELECT … FOR UPDATE`). Concurrent applications for the same class wait their turn, so the seat check and the insert happen one at a time. The lock is in PostgreSQL, so this also holds with several backend instances.
- **Automatic expiry:** every seat count and status read compares `expires_at` with the current time, so a seat is free the moment its hold expires. A background job also marks expired holds as `EXPIRED` in the database every 5 seconds.
- **Paying twice counts once:** confirm is a single conditional `UPDATE … WHERE status = 'HELD' AND expires_at > now`; a repeated confirm returns 200 without changing anything.

More details: [ASSUMPTIONS.md](ASSUMPTIONS.md). AI usage: [AI_USAGE.md](AI_USAGE.md).

## Local development (optional)

- Database: `docker compose up -d db`
- Backend: open `backend/SchoolAdmission.slnx` in Visual Studio and run the **http** profile (http://localhost:5080)
- Frontend: `cd frontend && npm install && npm run dev` (http://localhost:5173)
