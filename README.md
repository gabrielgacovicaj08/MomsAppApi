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

## Next recommended improvements

- Add test project (`MomsAppApi.Tests`) with smoke/integration tests
- Add migration/versioning guidance
- Add API endpoint docs (auth flows + sample requests)
