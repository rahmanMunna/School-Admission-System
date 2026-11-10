# AI Usage

## Tool

**Claude** (Anthropic), in chat.

## How I used it

I used AI as a pair programmer. I discussed design decisions with it when I was unsure. I got first drafts of code from it, which I then ran, tested, debugged and simplified myself. I also asked it to explain things until I could defend each part on my own.

**Design questions I discussed with it:**

- Background service vs a scheduler (Hangfire/Quartz) for expiring holds. I chose a .NET `BackgroundService`, because the job runs every 5 seconds, is idempotent, and isn't needed for correctness.
- How to stop overbooking: a C# `lock` vs SERIALIZABLE isolation vs a row lock. I chose `SELECT … FOR UPDATE` in a transaction, because it works across several backend instances.
- Polling vs WebSockets for live seat counts. I chose polling, because the brief asks for "every few seconds".
- 3-tier structure with repositories and a Unit of Work, whether a separate status table was needed (it wasn't), and nginx vs `serve` for the frontend.

**Code:** first drafts of the backend layers, the EF Core configuration, the services, the React pages, the Dockerfiles, and the backend and frontend automated tests came from AI. I built and ran each part step by step, and changed it where it didn't work or wasn't clear to me.

## What I did myself

- Ran everything in Visual Studio, VS Code and Docker, and tested every endpoint by hand: class full, duplicate apply, pay twice, wrong student, cancel, expiry without reload, refresh, server down.
- Solved environment problems on my machine. Port 8080 was taken by a Windows service, so I override the host port locally with `API_PORT` while the project default stays 8080. I also fixed a nested project folder.
- Refactored the code into a style I can explain. Services, repositories and controllers use classic constructors and plain `if`/`foreach`. I replaced a header-helper class with `[FromHeader]`, and removed extra frontend config files.
- Decided the assumptions in `ASSUMPTIONS.md`.

## Where the AI was wrong, and how I caught it

**1. The concurrency test failed in Docker.**
My concurrency test sends 200 applications at once to a 10-seat class. In Docker, the 10-seat limit held, but only 89 requests got 409 instead of 190, because many returned 500. The test setup allowed a pool of 200 connections, but PostgreSQL accepts only 100 by default. I set `Maximum Pool Size=40` for the tests and the backend, so extra requests now wait for a free connection instead of failing. This is also why two backend instances stay under PostgreSQL's limit.

**2. `GET /classes` crashed with "LINQ expression could not be translated".**
The repository projected classes into a `ClassSeatCounts` object first, then sorted and filtered that object. EF Core can't turn that into SQL. I caught it while testing the endpoint, and changed the query to sort and filter the `classes` table first and build `ClassSeatCounts` last.

**3. A frontend test found two "Continue" buttons.**
The Vitest setup was missing React Testing Library's `cleanup()`. Vitest doesn't expose `afterEach` globally by default, so each test's component stayed on the page and the next test saw two forms. I added `afterEach(cleanup)` in `setupTests.js`.

## How I verified the work

- Backend: 8 automated tests, including 2 concurrency tests, run in Docker as an evaluator would.
- Frontend: 10 Vitest tests (student ID rules, the student ID bar, API error handling).
- I removed `FOR UPDATE` temporarily to confirm the concurrency test fails without it, then put it back.
- I ran the whole stack from a fresh clone with `docker compose up --build`.
