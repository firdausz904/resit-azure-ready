# Resit backend

Modular monolith on .NET 10.

## Modules
- `Modules/Identity` — JWT issuing, PBKDF2 password hashing, `/api/auth/login`
- `Modules/Receipts` — upload, Hangfire-driven extraction jobs (with automatic retries), listing, dashboard totals
- `Modules/Bills` — reminders, recurrence, a daily Hangfire recurring job for due-date checks
- `Modules/Notifications` — SignalR hub shared by both modules

Every request pipeline threads a `CancellationToken` from the endpoint down through the
`Mediator` handler to the repository/HTTP call, and each Receipts/Bills route requires the
JWT's `household_id` claim to match the route's `{householdId}` segment (see
`Identity/HouseholdAuthorizationExtensions`).

In `Development`, the host calls `IdentityDbContext.Database.EnsureCreatedAsync()` and seeds one
household ("The Tan Family") with one user on first run:
- email: `weiling@family.com`
- password: `resit-dev-password`

## CQRS
Commands and queries use `Mediator` (martinothamar/Mediator), a source-generator-based, allocation-light alternative to MediatR. Each `Application` project defines `IRequest<TResponse>` records and their `IRequestHandler<TRequest, TResponse>`. Wire-up happens once in `Host/Resit.Api` via `AddMediator`; check the installed package version's README for the exact source-generator marker, since that detail shifts between major versions.

## Running locally
1. `createdb resit` (Postgres 16+). `IdentityDbContext` bootstraps itself via `EnsureCreated` in
   Development; for `ReceiptsDbContext` and `BillsDbContext`, add and apply a migration per
   module the first time (`dotnet ef migrations add Initial -c ReceiptsDbContext -s
   src/Host/Resit.Api`, same for `BillsDbContext`, then `dotnet ef database update` for each).
   Hangfire creates its own tables on first run.
2. Run the Python extraction service (`../python-extraction-service`) on `:8000`.
3. `dotnet run --project src/Host/Resit.Api` — Hangfire dashboard at `/hangfire`, SignalR hub at
   `/hubs/progress`, login at `POST /api/auth/login` with the seeded credentials below.
