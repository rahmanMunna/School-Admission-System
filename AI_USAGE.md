# AI Usage

**Tool:** Claude

## Where I used AI

| Part | How AI helped | What I did |
|---|---|---|
| Design decisions | Discussed options when I was unsure: row lock vs C# lock vs SERIALIZABLE, background service vs scheduler, polling vs WebSockets | Chose the approach and wrote the assumptions |
| Backend code | Generated the initial code for each layer |  Modified every layer: rewrote services, repositories and controllers and replaced a header-helper class with `[FromHeader]`, changed `ClassSeatCounts` to a class, fixed the EF query; tested every endpoint manually (class full, duplicate apply, pay twice, wrong student, cancel, expiry) |
| Frontend pages | First drafts of page structure and styling | Tested every flow in the browser, simplified config (removed extra config files) |
| Automated tests | First drafts of backend (xUnit) and frontend (Vitest) tests | Ran them, found and fixed the failures below |
| Docker | First drafts of Dockerfiles and compose | Fixed local port conflict (8080 taken) with `API_PORT`, kept 8080 as default |

## Where AI was wrong → how I caught it → how I solved it

1. **Concurrency test failed in Docker**
   - Wrong: test setup used `Maximum Pool Size=200`; 200 parallel requests exceeded PostgreSQL's 100 connections → many 500s.
   - Caught: Docker test run showed 89 conflicts instead of 190 (the 10-seat limit still held).
   - Solved: `Maximum Pool Size=40` for tests and backend, so extra requests wait instead of failing.

2. **`GET /classes` crashed: "LINQ expression could not be translated"**
   - Wrong: repository sorted/filtered *after* projecting into `ClassSeatCounts`.
   - Caught: testing the endpoint.
   - Solved: sort and filter the `classes` table first, project last.

3. **Frontend test found two "Continue" buttons**
   - Wrong: Vitest setup had no React Testing Library `cleanup()`, so earlier tests' components stayed on the page.
   - Caught: failing `StudentBar` test.
   - Solved: `afterEach(cleanup)` in `setupTests.js`.

## How I verified

- 8 backend tests (incl. 2 concurrency) and 10 frontend tests pass.
- Manual checks: class full, duplicate apply, pay twice, wrong student, cancel, expiry without reload, server down, data after restart.


