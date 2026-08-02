# Sultan Alomran Portfolio Platform

A personal engineering portfolio and technical-content platform built as two independent Angular clients and an ASP.NET Core Clean Architecture backend.

## Foundation status

1. **Solution foundation — complete.** The API composition root, dependency boundaries, public/client shells, automated tests, and CI validation are established.
2. **Complete data foundation — complete.** SQL Server and EF Core model the approved 45 entities for profile, taxonomy, Visual Handbook, projects, media, learning paths, engagement, analytics, authorization, tokens, and auditing.
3. **Data-foundation cleanup — in progress.** Entity mappings are colocated with their `IEntityTypeConfiguration<TEntity>` implementations, DbSet naming is consistent, committed credentials are removed, and migration validation is enforced once generated artifacts are available.

The model supports Arabic and other multilingual text through Unicode `nvarchar` columns. Selective soft deletion applies only to Category, Infographic, Project, Series, and ReadingPath, with active-row filtered uniqueness. Custom authorization persistence stores users, roles, permissions, junctions, sessions, and cryptographic token hashes—never raw tokens. Deterministic reference seed data contains roles, permissions, and safe configuration only; it contains no account, credential, secret, token, or analytics data.

## Structure and dependency direction

`Domain` and `Shared` are innermost. `Application` references them; `Infrastructure` owns EF Core and references the inner projects; `Portfolio.Api` is the composition root. The Angular applications communicate only through HTTP/API contracts.

```text
src/Portfolio.Api             API composition root
src/Portfolio.Application     use-case boundary
src/Portfolio.Domain          dependency-free domain model
src/Portfolio.Infrastructure  EF Core and technical adapters
src/Portfolio.Shared          shared .NET primitives
src/Portfolio.Web             public Angular client
src/Portfolio.Admin           private Angular client
tests/                        unit, architecture, metadata, and integration tests
```

## Database environments and configuration

Development requires SQL Server Developer Edition, compatible LocalDB, or a local SQL Server container. CI should use a disposable SQL Server container/database. Production targets Azure SQL Database; configuration belongs in Azure environment variables or Key Vault, with managed identity preferred when supported.

Supply `ConnectionStrings__PortfolioDatabase` locally. The committed `appsettings.json` deliberately contains an empty value. Development alternatives include environment variables, `dotnet user-secrets`, local Docker configuration, or an ignored developer-local settings file. Copy `.env.example` and replace `<LOCAL_PASSWORD>` locally; never commit it.

The intended initial migration is `InitialDataFoundation`. Migration artifacts remain pending where the pinned .NET 10 SDK is unavailable; see `docs/database/README.md` for the exact generation and validation commands. The API never applies migrations automatically at startup.

## Validation

```bash
dotnet restore Portfolio.sln
dotnet build Portfolio.sln --configuration Release --no-restore
dotnet test Portfolio.sln --configuration Release --no-build
dotnet format Portfolio.sln --verify-no-changes --no-restore
dotnet list Portfolio.sln package --vulnerable --include-transitive
dotnet ef migrations has-pending-model-changes --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Api
```

SQL Server integration tests require Docker or another isolated SQL Server. Metadata and architecture tests validate the model without touching production. NuGet auditing, warnings-as-errors, and NU1903 enforcement remain enabled.

## Next work

Future work must proceed as complete end-to-end vertical slices rather than another horizontal foundation. The recommended first slice is `feature/projects`, covering one coherent project capability from persistence through API contract and user experience without broad unrelated redesign.
