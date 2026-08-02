# SQL Server data foundation

## Overview

The code-first EF Core model contains 45 tables across authorization, professional profile, taxonomy, Visual Handbook, projects, reusable media, reading paths, engagement, contact, analytics, and system/token groups. `PortfolioDbContext` exposes one set and Infrastructure provides one Fluent API configuration per entity.

Key relationships include ordered `SeriesItems`, polymorphic `ReadingPathItems`, reusable `MediaFiles` for project imagery and professional assets, role/permission junctions, and authenticated engagement. SQL Server cannot enforce polymorphic `EntityType`/`EntityId` references; application validation must constrain them to Infographic, Project, Category, Series, or ReadingPath.

## Approved decisions

All bilingual/human text is `nvarchar`, with centralized bounded lengths and `nvarchar(max)` for long content. There are no parallel Arabic/English columns or translation tables. Articles and `PublicCount` are excluded. `EntityStatistics` is only a denormalized cache; events remain authoritative.

Indexes enforce unique identities, vocabularies, active slugs/names, junction pairs, positions, hashes, settings, session identifiers, storage paths, and statistic pairs. Named checks enforce ratings, positions, dimensions, counters, percentages, averages, display orders, and date ranges. Cascades are limited to owned children; taxonomy, technology, and media references restrict deletion; history commonly sets nullable user FKs to null or uses NoAction where SQL Server cascade paths require it.

## Soft deletion and restore

Category, Infographic, Project, Series, and ReadingPath are selectively filtered by `IsDeleted`. Filtered indexes (`WHERE [IsDeleted] = 0`) preserve active-row uniqueness. A future admin recovery workflow will use `IgnoreQueryFilters()`, resolve active-key conflicts, then call `Restore()`. Tracking, audit, tokens, sessions, and junctions are never hidden.

## Privacy and tokens

IP address, user agent, referrer, country, session identifier, device, and browser fields are sensitive. Future ingestion must apply consent, least privilege, and retention policy. Token tables persist cryptographic hashes only—never raw tokens. No credentials or users are seeded.

## Professional profile and seed behavior

A singleton Profile can reference profile-image and CV media; ExperienceItems are date/display ordered, Skills belong to SkillCategories, and Certifications may reference media and a verification URL. Deterministic seed infrastructure supplies only the Administrator role, non-secret permissions, and a safe culture setting. Administrator account/bootstrap credentials are deferred.

## Migration workflow

The current execution environment does not include the .NET SDK, so generated migration artifacts are intentionally pending rather than fabricated. With .NET 10 installed, run:

```bash
dotnet tool restore
dotnet ef migrations add InitialDataFoundation --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Api --output-dir Persistence/Migrations
dotnet ef migrations script --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Api --idempotent
dotnet ef database update --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Api
```

Never migrate production automatically at API startup. Configure `ConnectionStrings__PortfolioDatabase` through local secrets, environment variables, a secret manager, or Azure Key Vault. For every future schema change, update entities/configurations, add metadata and SQL Server tests, generate and review a named migration and idempotent SQL, then synchronize this guide and `ERD.dbml`.

## Cleanup validation status

Entity-specific relationships, indexes, filters, constraints, precision, and delete behavior now live in the corresponding configuration class; the former centralized relationship switch has been removed. DbSet CLR property names use predictable plurals while existing explicit table mappings remain stable. The base API configuration has no database credential, and local configuration must be supplied externally.

Migration generation, idempotent-script review, and live SQL Server execution remain required before this cleanup milestone can be declared complete. They were not fabricated in an environment without the pinned .NET 10 SDK. CI retains a temporary non-blocking pending-model check solely until `InitialDataFoundation` and `PortfolioDbContextModelSnapshot` are generated and committed.
