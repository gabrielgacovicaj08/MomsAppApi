# momsappapi — Progress

Last updated: 2026-02-27

## Current phase
Security/Auth hardening (PR 1 prep)

## Done
- Repo structure + stack review completed
- README/run-path reviewed
- Auth flow reviewed (`Program.cs`, `AuthService`, `AuthController`)
- API surface scan for authorization and HTTP verb risks

## Findings (high priority)
- JWT secret committed in `appsettings.json`
- DB connection string committed in `appsettings.json`
- `app.UseAuthentication()` missing before `UseAuthorization()`
- Mixed local/UTC token time handling
- Sensitive endpoints with weak/disabled authorization patterns

## In progress
- Preparing reviewable diff summary

## Completed in this pass
- Added `app.UseAuthentication()` before `UseAuthorization()` in `Program.cs`
- Added startup validation for missing `AppSettings:Token`
- Replaced committed secret placeholders in `appsettings.json` (`Token` + `MomsAppDb` now blank)
- Normalized token timestamps to UTC in `AuthService`
- Updated README with `dotnet user-secrets` setup instructions

## Completed in this pass
- Added `[Authorize]` at controller level for Assignment, Employee, Structure, and WorkLog controllers
- Restricted mutation endpoints to admin where appropriate (`create/update/delete/deactivate`)
- Added backward-compatible RESTful routes in parallel:
  - `PUT employee/{employee_id}` alongside existing update route
  - `PATCH employee/{employee_id}/deactivate` alongside existing GET route
  - `PUT structure/{id}` alongside existing update route
  - `DELETE structure/{id}` alongside existing GET delete route
- Restricted worklog creation to authenticated `ADMIN` or `WORKER`

## Next
1. Validate endpoint behavior against frontend flows
2. Add policy for "worker can only fetch own assignments" (optional hardening)
3. Provide PR-ready commit grouping

## Blockers
- None

## Notes for Gabriel
- DB schema/data folder pending for deeper data-model review.
