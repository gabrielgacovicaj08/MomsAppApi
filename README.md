# MomsApp API

ASP.NET Core Web API for MomsApp.

## Stack
- .NET 9 (`net9.0`)
- ASP.NET Core
- Entity Framework Core (SQL Server)
- JWT authentication

## Run locally

```bash
dotnet restore
dotnet build
dotnet run
```

Default local URL is defined by `Properties/launchSettings.json`.

## Configuration

Core settings live in:
- `appsettings.json`
- `appsettings.Development.json`

Sensitive values should come from environment variables or user-secrets.

### CORS origins

Configure browser clients via `Cors:AllowedOrigins` (string array). Example:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5173",
    "https://app.momsapp.com"
  ]
}
```

In container/host environments, set this through environment variables (for example `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ...).

### Database resiliency (transient SQL failures)

EF Core SQL Server now uses built-in transient retry handling to reduce failed requests during short DB/network blips.

Configurable settings:

```json
"Database": {
  "CommandTimeoutSeconds": 30,
  "MaxRetryCount": 5,
  "MaxRetryDelaySeconds": 10
}
```

Environment variable equivalents:
- `Database__CommandTimeoutSeconds`
- `Database__MaxRetryCount`
- `Database__MaxRetryDelaySeconds`

### Local user-secrets setup (recommended)

```bash
dotnet user-secrets init
dotnet user-secrets set "AppSettings:Token" "<your-long-random-jwt-secret>"
dotnet user-secrets set "ConnectionStrings:MomsAppDb" "Server=<SERVER>;Database=<DB>;Trusted_Connection=True;TrustServerCertificate=true"
```

You can also set these as environment variables in shared environments.

## Suggested workflow

1. Create feature branch
2. Implement change
3. Run:
   - `dotnet format` (if configured)
   - `dotnet build`
   - `dotnet test` (when test project is added)
4. Open PR for review

## Security hardening

- Auth endpoints (`/api/Auth/login`, `/api/Auth/refresh-token`) use IP-based fixed-window rate limiting.
- Default policy: max **8 requests per minute per IP** (returns HTTP `429` when exceeded).
- Refresh tokens are now stored as SHA-256 hashes in the database (raw token is only returned once to the client), reducing blast radius if DB data is exposed.

## Operational health endpoints

Two unauthenticated health endpoints are available for load balancers, uptime checks, and deployment gates:

- `GET /health/live` → process-level liveness (API process is running)
- `GET /health/ready` → readiness including DB connectivity (`MomsAppDb`)

`/health/ready` returns `503` when DB is unavailable, allowing infra to fail fast instead of routing user traffic to a broken instance.

## API error handling

The API now returns standardized `application/problem+json` responses for unhandled errors and bare status-code failures.

- Production uses a global exception handler (`500`) with safe details.
- All problem responses include `traceId` and `timestamp` for faster support/debugging.
- Development keeps the detailed developer exception page.

## Request telemetry and traceability

Each request now logs a single structured line with:
- method + path
- final status code
- request duration (ms)
- `traceId`
- `correlationId` (propagated via `X-Correlation-Id` when provided by the client)
- caller context (`employee_id` claim when authenticated, plus role)

The API returns both `X-Trace-Id` and `X-Correlation-Id` on responses, and includes `correlationId` in problem-details errors. This lets frontend/support trace a user issue end-to-end across browser logs, API responses, and server logs.

## HTTP method safety hardening

Mutating endpoints no longer accept `GET` aliases.

- Structure deletion now requires `DELETE` (`/api/Structure/structure/{id}` or `/api/Structure/delete-structure/{id}`)
- Employee deactivation now requires `PATCH` (`/api/Employee/employee/{employee_id}/deactivate`)

This prevents accidental destructive actions from crawlers/link previewers and aligns endpoint behavior with standard REST expectations.

## Worklog guardrails (payroll integrity)

Worklog creation now enforces time sanity checks before writing to DB:

- `ended_at` must be later than `started_at`
- shift duration is capped (default: **16 hours**)
- timestamps cannot be in the future beyond a small clock-skew allowance (default: **10 minutes**)

Config:

```json
"WorkLog": {
  "MaxShiftHours": 16,
  "FutureClockSkewMinutes": 10
}
```

Environment variable equivalents:
- `WorkLog__MaxShiftHours`
- `WorkLog__FutureClockSkewMinutes`

## Next recommended improvements

- Add test project (`MomsAppApi.Tests`) with smoke/integration tests
- Add migration/versioning guidance
- Add API endpoint docs (auth flows + sample requests)
