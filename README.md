# Sultan Alomran Portfolio Platform

A premium personal engineering portfolio and technical-content showcase. The solution contains an independently deployable public Angular application, private Angular CMS shell, and ASP.NET Core API, following the approved Clean Architecture and feature-oriented boundaries.

## Current milestone

**Solution foundation** establishes buildable hosts, dependency boundaries, local configuration, validation tests, and CI. The visible Angular content is deliberately a minimal responsive shell, not the approved homepage or admin dashboard.

## Solution structure

```text
src/
├── Portfolio.Api             ASP.NET Core API host
├── Portfolio.Application     use-case boundary
├── Portfolio.Domain          dependency-free domain boundary
├── Portfolio.Infrastructure  technical adapters boundary
├── Portfolio.Shared          narrowly shared .NET primitives
├── Portfolio.Web             public Angular + Tailwind shell
└── Portfolio.Admin           private Angular shell
tests/
├── Portfolio.ArchitectureTests
├── Portfolio.UnitTests
└── Portfolio.IntegrationTests
```

### Dependency direction

`Domain` and `Shared` are innermost. `Application` references `Domain` and `Shared`. `Infrastructure` references `Application`, `Domain`, and `Shared`. The API composition root references `Application`, `Infrastructure`, and `Shared`. Neither Angular app shares C# DTOs or accesses persistence. Future capabilities should be cohesive vertical slices while preserving these compile-time directions.

## Frontend version strategy

| Application | Angular | TypeScript | Styling | Node.js |
|---|---|---|---|---|
| Portfolio.Web | Stable 22 (`^22.0.0`) | Stable `>=6.0.0 <6.1.0` | Tailwind CSS 4.1.14+, custom public design | 22 LTS (`>=22.12.0 <23`) |
| Portfolio.Admin | Stable 20.3.7 | 5.9.3 | Tailwind CSS 4.1.14+, future Metronic Tailwind integration | 22 LTS (`>=22.12.0 <23`) |

The version difference is intentional. The applications have independent manifests, future lockfiles, tool configurations, builds, deployments, and upgrade reviews; they do not share Angular runtime code or dependencies. Both consume `Portfolio.Api` through HTTP/OpenAPI contracts. `Portfolio.Web` uses the current stable Angular major for its custom experience, while `Portfolio.Admin` stays on Angular 20 because the current official Metronic Tailwind Angular integration example targets Angular 20 with Tailwind CSS v4+.

The .NET SDK is pinned to 10.0.100 with latest-patch roll-forward and previews disabled. Node.js 22 LTS is the common runtime range selected for both frontends rather than an unnecessarily exact patch.

## Prerequisites

Install the pinned .NET SDK and a Node.js version in each application's declared `engines` range. Trust the ASP.NET Core development HTTPS certificate when running HTTPS locally. No database, storage emulator, credentials, or secrets are required for this milestone.

## Restore, build, and test

```bash
dotnet restore Portfolio.sln
dotnet build Portfolio.sln --configuration Release --no-restore
dotnet test Portfolio.sln --configuration Release --no-build

npm --prefix src/Portfolio.Web ci
npm --prefix src/Portfolio.Web run build
npm --prefix src/Portfolio.Admin ci
npm --prefix src/Portfolio.Admin run build
```

Run all available validation with `./scripts/validate.sh`. Formatting can be checked with `dotnet format Portfolio.sln --verify-no-changes --no-restore`; each Angular project supports `npm run lint`.

Each Angular application requires its own `package-lock.json`. These lockfiles must be generated and committed separately from an environment with npm registry access before `npm ci`, Angular builds, or Angular CI validation can run. CI reports this temporary limitation and automatically enables each application's install, lint, and production-build steps when its lockfile exists; it does not substitute `npm install` for reproducible validation.

## Run locally

| Application | Command | Local URL |
|---|---|---|
| API | `dotnet run --project src/Portfolio.Api` | `http://localhost:5100` / `https://localhost:7100` |
| API health | — | `http://localhost:5100/health` |
| OpenAPI (Development) | — | `http://localhost:5100/openapi/v1.json` |
| Public web | `npm --prefix src/Portfolio.Web start` | `http://localhost:4200` |
| Admin | `npm --prefix src/Portfolio.Admin start` | `http://localhost:4300` |

Configuration is layered through `appsettings.json` and environment-specific ASP.NET Core files. Override non-secret values with environment variables such as `Cors__AllowedOrigins__0`. Angular development and production environment files define only the public API base URL.

## Intentionally not implemented

This milestone contains no database entities, DbContext, EF Core packages/configuration, migrations, seed data, repositories, authentication/authorization, JWTs, business APIs or services, portfolio features, analytics, uploads, full homepage, dashboard, Metronic assets, Azure deployment, or persistence. Those require their dedicated approved milestones.

## Next milestone

`feature/persistence-foundation`, after the blocking database decisions in `docs/Database_Specification.md` are resolved.

## Complete data-foundation milestone

The complete 45-entity SQL Server/EF Core persistence model is now established, including the professional profile, Visual Handbook, projects, reusable media, learning journeys, engagement, analytics, custom authorization/token persistence, auditing, constraints, deterministic non-secret seed data, and an EF database health check. Articles and `PublicCount` are intentionally excluded; administrator account bootstrap and all authentication/business APIs remain deferred.

Install SQL Server and .NET 10, then provide `ConnectionStrings__PortfolioDatabase` (or `ConnectionStrings:PortfolioDatabase` in local configuration). All bilingual fields use Unicode `nvarchar`; no duplicate language columns are used. Selective soft deletion applies only to Category, Infographic, Project, Series, and ReadingPath.

Migration generation is pending in this SDK-less execution environment. Generate and apply it with:

```bash
dotnet ef migrations add InitialDataFoundation --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Api
```

The deterministic seed contains only a role, permissions, and a safe setting—never users, credentials, secrets, raw tokens, analytics, or public claims. Run `dotnet test Portfolio.sln --configuration Release` for domain, architecture, metadata, and integration validation. SQL Server container tests require Docker.
