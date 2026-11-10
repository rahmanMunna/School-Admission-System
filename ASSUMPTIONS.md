# Assumptions

| # | Assumption | Reason |
|---|---|---|
| 1 | Many guardians applying for the same class at once is handled by a database transaction + row-level lock (`SELECT … FOR UPDATE` on the class row) | Requests for one class take turns, so seats can never exceed `totalSeats`; the lock is in PostgreSQL, so it also works with several backend instances |
| 2 | Pay and cancel are each one conditional `UPDATE … WHERE status = 'HELD' AND expires_at > now` | Only one request can change the row, so double clicks and races with expiry have exactly one result |
| 3 | Connection pool: max 40 connections per backend instance | PostgreSQL allows 100 by default; extra requests wait instead of failing, even with 2 instances |
| 4 | A hold is expired when `now >= expires_at` | One clear boundary; paying must happen before it |
| 5 | A seat is freed at `expires_at` without any API call: every read only counts HELD holds with `expires_at > now`. A background job also marks expired rows `EXPIRED` every 5 s | The seat comes back at the exact second; the job only keeps stored data accurate |
| 6 | Polling: class list every 3 s, application pages every 5 s; countdown every 1 s in the browser | Brief asks for updates "every few seconds"; simpler than WebSockets |
| 7 | Pay twice → 200 both times, counted once | Safe to retry after a double click or network error |
| 8 | Cancel twice → 204; cancel a paid admission → 409; cancel an expired hold → 409 | Retry is safe; a paid admission can't be undone here; the UI can explain why |
| 9 | No login: the student ID is an identity claim, not proof; the admin page is open | Brief says no login; production would need guardian OTP and staff accounts |
| 10 | A student can apply again after EXPIRED or CANCELLED | Only active (HELD/CONFIRMED) applications are limited |
| 11 | A student can hold seats in different classes at the same time | The brief limits one active application per class only |
| 12 | Pay = instant success; no amount is stored | Brief says to assume payment succeeds |
| 13 | Duplicate class names allowed; name 1–100 characters; seats 1–10,000 | Re-running tests must not fail; limits block bad input |
| 14 | Used Docker Volume | For data persistent. If remove or crash the container, again spin up a container , and data remains same |
