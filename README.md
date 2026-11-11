# School Admission System


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

| Variable | Default |
|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` |
| `API_PORT` | `8080` |
| `FRONTEND_PORT` | `3000` | 
| `API_BASE_URL` | `http://localhost:8080` | 
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | `admission` | 
| `DB_PORT` | `5433` | 

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





