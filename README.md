# Resit

A household receipt tracker: snap or upload receipts, extract merchant/total/date offline via
PaddleOCR, watch batch progress live over SignalR, and keep an eye on bills and loans.

## Layout

```
backend/                  .NET 10 modular monolith (CQRS via Mediator, SignalR, Hangfire)
frontend/                 React + TypeScript, mobile-responsive, SignalR client
python-extraction-service/ FastAPI + PaddleOCR receipt extraction
```

## Running everything locally

1. **Postgres** — `createdb resit`, then run EF Core migrations for both `ReceiptsDbContext`
   and `BillsDbContext` (see `backend/README.md`) and Hangfire's own schema will self-create
   on first run.
2. **Extraction service** — `cd python-extraction-service && pip install -r requirements.txt
   && uvicorn main:app --port 8000`.
3. **Backend** — `cd backend && dotnet run --project src/Host/Resit.Api` (defaults to
   `http://localhost:5000`; Hangfire dashboard at `/hangfire`).
4. **Frontend** — `cd frontend && npm install && npm run dev` (`http://localhost:5173`, proxies
   `/api` and `/hubs` to the backend).

## Notes on scope

This is a working skeleton, not a finished product — auth is a JWT-bearer stub (no `/auth`
endpoint implemented yet), and there's no household/user registration flow. The parts that matter
for the architecture you asked for are wired end to end:

- Uploading a batch enqueues each file onto an in-process bounded-concurrency queue, calls the
  Python service per file, and pushes `BatchProgressUpdated` / `ReceiptProcessed` events over
  SignalR as each one finishes.
- Bills use a Hangfire recurring job (`bill-reminder-check`, daily) that scans due dates and
  pushes a `ReminderDue` SignalR event to the household's group.
- Every handler and repository call takes a `CancellationToken` through to the database and
  HTTP calls.
