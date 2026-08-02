# Sultan Alomran Portfolio Platform — Implementation Plan

**Status:** Official implementation baseline; this document authorizes planning only, not application implementation, scaffolding, project creation, or repository reorganization.

**Prepared:** 2 August 2026
**Source-of-truth inputs reviewed:** the repository's Project 00 master document (`Project_00_Master_Document_to_Give_Figma_AI 2.md`), ERD (`D1036970-646E-4B76-89A1-E280BDA8A0E8.png`), homepage reference (`IMG_0266.jpeg`), roadmap (`AB5208DD-3E05-4E0F-8D04-BF8DC1F92E51.png`), and branching workflow (`C80D222F-58C0-4BB7-9B10-BEBD761CF5C0.png`). These are the files currently present under generated names and correspond to the five artifacts named in the project brief.

The governing implementation inputs are the Project 00 Master Document, the finalized Database Specification, the ERD, the Figma Design System, and approved UI screens. Where an input is not yet finalized or present, this plan records the dependency rather than inventing a decision.

## 1. Executive Summary

The product is a premium, content-managed engineering portfolio for Sultan Alomran. It has two equally important public pillars—enterprise **Projects / Case Studies** and a **Visual Handbook / Technical Content** library—supported by professional profile, experience, certifications, search, contact, and measured engagement. It is explicitly not a course platform, social network, generic blog, or generic dashboard.

The target solution consists of three separately deployable applications:

- **Portfolio.Web:** a custom, SEO-ready public Angular application using Tailwind CSS.
- **Portfolio.Admin:** a private Angular CMS using Metronic conventions.
- **Portfolio.Api:** an ASP.NET Core Web API using EF Core and SQL Server, with authentication, media integration, analytics, and content-management capabilities.

Implementation should proceed in complete, testable vertical slices after the source-of-truth artifacts are approved. The MVP is deliberately limited to Homepage, Projects, Visual Handbook, About, Experience, Contact, a basic single-administrator CMS, authentication, media upload, SEO, accessibility, and a security baseline. Learning Paths, Articles, Notifications, advanced analytics, recommendations, multi-user administration, advanced roles, media collections, bookmarks, and community capabilities are future enhancements.

## 2. Solution Architecture

### 2.1 Logical context

```text
Public visitors ──> Portfolio.Web ─┐
                                  ├──> Portfolio.Api ──> SQL Server
Administrator ───> Portfolio.Admin ┘         │
                                             ├──> Blob/object storage
                                             ├──> Email provider
                                             └──> Internal engagement events

Portfolio.Web / Portfolio.Admin ──> External behavioral analytics (Clarity or GA4)
```

Both Angular clients consume versioned HTTP contracts; neither accesses persistence directly. SQL Server holds structured data and file metadata. Azure Blob Storage or Cloudinary holds file bytes. A provider decision is required before media implementation.

### 2.2 Architecture principles

- Apply **Clean Architecture dependency rules** at solution boundaries and **feature-oriented vertical slices** inside the API.
- Keep domain and application logic independent of HTTP, EF Core, storage, email, and analytics providers.
- Use thin API endpoints/controllers; use-case handlers own validation, authorization, orchestration, and transaction boundaries.
- Publish DTOs/contracts, never EF Core entities. Avoid a speculative shared project unless it provides clear value.
- Use REST conventions, explicit error contracts, server-side pagination/filtering, cancellation, and idempotency where needed. Design the API so versioning can be introduced cleanly in the future. Explicit API versioning should be added when multiple client versions or external consumers require it.
- Treat accessibility, privacy, SEO, performance, observability, and secure defaults as acceptance criteria rather than later polish.
- Separate environments and configuration; keep secrets outside source control.

### 2.3 Cross-cutting capabilities

- Authentication/authorization for the admin surface, short-lived access plus securely rotated refresh credentials (or secure cookie-based browser authentication after threat-model review), rate limiting, and audit trails.
- Central validation, RFC 9457-style problem responses, exception mapping, structured logs, correlation IDs, health/readiness checks, and essential metrics.
- Output caching for safe public reads; cache invalidation after publishing; CDN delivery and optimized variants for public media.
- Content security policy, strict CORS allowlists, anti-forgery protection when cookies are used, secure headers, input sanitization, file type/size validation, and contact-form abuse protection.
- OpenAPI as the API contract source; generate typed Angular clients only after contract review.

## 3. Repository Structure

### 3.1 Approved target organization

```text
/
├── README.md
├── CONTRIBUTING.md
├── Portfolio.sln
├── docs/
├── deploy/
├── scripts/
├── tests/
└── src/
    ├── Portfolio.Api
    ├── Portfolio.Web
    ├── Portfolio.Admin
    ├── Portfolio.Domain
    ├── Portfolio.Application
    ├── Portfolio.Infrastructure
    └── Portfolio.Shared
```

This is the approved repository layout for all later implementation. `Portfolio.Api`, `Portfolio.Web`, and `Portfolio.Admin` remain independently buildable and deployable. `Portfolio.Shared` must contain only genuinely shared .NET contracts or primitives; it must not become a general-purpose dependency or expose EF Core entities. This plan does not create the structure.

### 3.2 Current organization review

The repository currently contains planning/reference assets at its root plus a short README; no application structure exists. This is appropriate at the planning stage. Generated asset names are difficult to discover, so a later, separately approved cleanup should use the following documentation structure:

```text
docs/
├── Project_00_Master_Document.md
├── Implementation_Plan.md
├── Database_Specification.md
├── ERD.png
├── Roadmap.png
├── Workflow.png
└── ui/
    └── Homepage_Approved.png
```

This is a recommendation only. Do **not** rename or move existing files as part of this plan. Preserve current paths until the owner approves the cleanup and any external references are updated.

## 4. Folder Structure

### 4.1 ASP.NET Core internals

```text
src/Portfolio.Api/
├── Features/
│   ├── Auth/
│   ├── Home/
│   ├── Projects/
│   ├── Infographics/
│   ├── Taxonomy/
│   ├── Series/
│   ├── Profile/
│   ├── Search/
│   ├── ContactMessages/
│   ├── Media/
│   ├── Analytics/
│   └── Settings/
├── Infrastructure/                # EF, identity, storage, email, telemetry
├── Common/                        # narrow, truly cross-cutting behavior
├── Program.cs
└── appsettings.json               # non-secret defaults only
src/
├── Portfolio.Domain/              # entities, value objects, domain rules
├── Portfolio.Application/         # use cases and ports
├── Portfolio.Infrastructure/      # EF Core and external adapters
└── Portfolio.Shared/              # narrowly shared .NET contracts/primitives
tests/
├── Portfolio.ArchitectureTests/
├── Portfolio.UnitTests/
└── Portfolio.IntegrationTests/
```

Prefer cohesion over a fixed project count: establish dependency boundaries, then extract projects only where they improve enforceability. Each API feature owns endpoints, contracts, validation, handlers, and tests; persistence configuration remains centralized enough to protect schema consistency.

### 4.2 Angular application internals

```text
src/app/
├── core/                           # app-wide providers, auth, HTTP, errors, config
├── layout/                         # application shell
├── shared/
│   ├── ui/                         # presentational primitives
│   ├── models/                     # client-facing types
│   ├── utilities/
│   └── accessibility/
└── features/
    └── <feature>/
        ├── pages/
        ├── components/
        ├── data-access/
        ├── models/
        └── <feature>.routes.ts
```

For the MVP, public features are Home, Projects, Visual Handbook and infographic details, Experience, About, Contact, and Not Found. The basic admin features are Auth, Dashboard, Projects, Infographics, Taxonomy required by those content types, and Media Upload. Search, Series-specific experiences, advanced analytics, notifications, recommendations, bookmarks, and multi-user administration are future enhancements. Sharing between the Angular applications should be limited to generated API contracts and intentionally packaged design-agnostic utilities; their visual systems are different.

## 5. Development Milestones

| Milestone | Outcome | Exit criteria |
|---|---|---|
| **0. Scope and design readiness** | Resolve contradictions, approve responsive designs, component states, content inventory, API/data decisions, and non-functional targets. | Open questions answered; designs and technical handoff approved; privacy review complete. |
| **1. Repository and solution foundation** | Buildable solution, two Angular applications, API host, formatting/linting, tests, OpenAPI, logging, configuration, health checks, local dependencies, CI. | Clean clone can build/test deterministically; no production secrets; architecture checks pass. |
| **2. Persistence and platform foundation** | Reconciled written database specification and ERD, EF configurations/migrations, seed strategy, storage abstraction, authentication baseline, and essential telemetry. | Reviewed initial migration works on a fresh database and through the documented upgrade path. |
| **3. Public foundation and homepage** | Design tokens, responsive shell, navigation/footer/theme, home API composition, homepage, SEO metadata, loading/error/empty states. | Approved desktop/tablet/mobile parity; accessibility and performance budgets pass. |
| **4. Projects vertical slice** | Listing, filters/pagination, details/case studies, links/images/technologies, admin CRUD and publish workflow. | End-to-end author-to-public flow tested; confidentiality checks enforced. |
| **5. Visual Handbook vertical slice** | Categories/tags, listing/filtering, infographic viewer, media upload, and basic admin editing/publishing. | Viewer is accessible and responsive; uploads and author-to-public workflow are tested. |
| **6. About, Experience, and Contact** | Approved professional profile, experience, contact form, and essential admin-managed content. | Approved facts only; accessible forms, abuse controls, and delivery behavior verified. |
| **7. Basic Admin CMS completion** | Single-admin content management, authentication, project/infographic publishing, media upload, and essential dashboard status. | Authorization, validation, publishing, and media workflows pass end-to-end tests. |
| **8. MVP hardening and release** | SEO, security baseline, accessibility, performance, cross-browser/responsive QA, backup, essential monitoring, runbooks, and staging UAT. | Release checklist signed; critical issues closed; application rollback procedure verified. |
| **9. Future enhancements** | Learning Paths, Articles, Notifications, advanced analytics, recommendations, multi-user administration, advanced roles, media collections, bookmarks, and community features. | Each capability receives separately approved scope, design, schema, and milestone. |

Every milestone must include implementation, automated tests, documentation, observability, and review; “UI complete” alone is not complete.

## 6. Git Branching Strategy

Adopt the supplied milestone workflow **after** repository setup is authorized:

- `main` is protected, stable, production/release-ready, and never receives direct implementation commits.
- `dev` is the protected integration branch.
- Create short-lived `feature/<milestone-or-slice>` branches from current `dev`; one branch delivers one cohesive milestone or vertical slice.
- Use focused Conventional Commits, push a pull request to `dev`, require green CI and review, squash or merge according to the selected history policy, then delete the feature branch.
- Promote tested release candidates from `dev` to `main` through a pull request; tag immutable releases (for example, `v1.0.0`).
- Create `hotfix/<issue>` from `main` only for urgent production fixes, then merge the fix back into both `main` and `dev`.
- Protect `main` and `dev` with required status checks, review, resolved conversations, no force-pushes, and environment approval for production.

The recommended development workflow is:

```text
main
└── dev
    ├── feature/solution-foundation
    ├── feature/persistence-foundation
    ├── feature/public-foundation
    ├── feature/homepage
    ├── feature/projects
    ├── feature/visual-handbook
    ├── feature/about
    ├── feature/experience
    ├── feature/contact
    ├── feature/admin-foundation
    ├── feature/admin-content
    ├── feature/analytics
    └── feature/deployment
```

The `feature/analytics` branch is reserved for a later analytics milestone unless the MVP needs only minimal operational counters. Every feature branch must deliver a complete vertical slice—database, API, Angular, testing, and documentation—and leave the integrated application working. Page-only branches are not acceptable. A slice may be divided into smaller independently working branches when reviewability requires it. **No branch is created by this planning deliverable.**

## 7. Database Generation Strategy

### 7.1 Authority of the written specification

The **ERD is the visual database reference** for entities and relationships. `Database_Specification.md` is the **authoritative written database specification** once finalized. Future implementation must follow the written specification because it records constraints, field semantics, nullability, defaults, indexes, lifecycle rules, security considerations, and other details that cannot be represented fully in the ERD image.

If the two artifacts conflict, stop implementation and update or clarify the written specification before generating entities or migrations. The approved source priority for database work is `Database_Specification.md`, then the ERD as its visual companion, with the Project 00 master document defining product scope.

### 7.2 Reconcile before generating

The ERD is a detailed logical starting point, not a license to generate unchecked code. First reconcile it with the master document. The ERD includes `LearningPaths`, `LearningPathItems`, code examples/resources/steps, bookmarks, user interactions, articles as polymorphic types, and several token/session models that are either optional, deferred, or potentially inconsistent with the explicit non-goal of becoming a learning platform. Conversely, the master document calls for profile, experience, skills, certifications, project links, and audit behavior that need confirmation against the pictured schema.

Produce an approved data dictionary containing ownership, nullability, lengths, enum values, defaults, uniqueness, delete behavior, privacy class, retention, indexes, and audit rules before writing entities or a migration.

### 7.3 EF Core approach

1. Model approved entities in code and use EF Core Fluent API configurations; do not expose them as API contracts.
2. Use GUID keys with an explicitly chosen sequential-generation mechanism compatible with SQL Server. Use UTC timestamps and an application-wide time abstraction.
3. Enforce unique slugs/usernames/emails where required; composite uniqueness on join/order tables; indexes for foreign keys, status, publish dates, sort order, search/filter columns, and event lookup patterns.
4. Use normalized joins for project technologies, infographic tags, series items, roles, and permissions. Avoid an unconstrained polymorphic `EntityType`/`EntityId` design until referential-integrity and query requirements are accepted; prefer typed event semantics or carefully validated references.
5. Apply soft delete only to managed content that needs recovery. Define filtered unique indexes, query-filter behavior, restore rules, and physical purge/retention separately. Never silently soft-delete security tokens or high-volume telemetry.
6. Keep media bytes out of SQL Server. Store provider key, URL policy, MIME type, byte size, dimensions, checksum, alt text, ownership/usage, and lifecycle state; generate public URLs through the storage abstraction.
7. Create a hand-reviewed initial migration. CI must create a database from zero, apply every migration in order, run integration tests, and generate a reviewed idempotent deployment script/bundle.
8. Seed only deterministic reference/configuration data and an opt-in development administrator. Inject real production admin credentials securely during deployment; never commit them.
9. Back up before production migrations, use backward-compatible expand/migrate/contract changes, deploy migrations as a controlled release step rather than API startup side effects, and document rollback/forward-fix procedures.
10. Load realistic, non-sensitive development fixtures separately from migrations. Test pagination, concurrency, cascade behavior, uniqueness, publish scheduling, deletion, analytics aggregation, and recovery.

## 8. Angular Architecture

### 8.1 Shared standards

- Use the current supported Angular release selected and pinned at kickoff, standalone APIs, strict TypeScript/template checks, route-level lazy loading, and feature-based boundaries.
- Prefer signals for local/derived UI state and RxJS for asynchronous streams. Add a global state library only when demonstrated cross-feature complexity justifies it.
- Centralize API base URL/configuration, authentication, error mapping, correlation headers, and generated clients in `core`/`data-access`; components never hand-build endpoint URLs.
- Use reactive forms with shared accessible validation patterns. Provide skeleton, empty, error, success, offline, and permission states specified by the master document.
- Enforce responsive behavior at desktop/tablet/mobile breakpoints, keyboard operation, visible focus, semantic headings, reduced-motion preferences, adequate targets, and automated plus manual accessibility testing.

### 8.2 Portfolio.Web

- Use Tailwind CSS backed by named design tokens: violet `#7C3AED`, indigo `#4F46E5`, ink `#0F172A`, page `#F8FAFC`, white surface, slate text `#64748B`, border `#E2E8F0`, Inter, an 8px spacing rhythm, and restrained depth/radii.
- Reproduce the homepage reference's composition—not its unverified claims. The reference's “40+ data entities,” “50+ infographics,” “100% passion,” sample project imagery, and Login call-to-action must not become factual content without approval/data support.
- Select SSR/prerendering during Milestone 0. The recommended default is SSR or hybrid rendering for indexable detail/listing routes, with hydration and route data/meta resolvers; authenticated admin routes do not need SSR.
- Use optimized responsive images, lazy loading, stable aspect ratios, `@defer` below the fold, canonical URLs, Open Graph metadata, structured data where valid, sitemap/robots generation, and explicit 404 behavior.
- Make filters URL-addressable so navigation, sharing, analytics, and server rendering remain predictable.

### 8.3 Portfolio.Admin

- Use a Metronic-based, desktop-first shell while preserving essential tablet/mobile operations.
- Guard routes and actions using server-backed authorization; UI hiding is not security. Support session expiry, unsaved-change guards, validation summaries, upload progress/retry, publish confirmation, and preview.
- Favor server-driven tables with pagination/filtering/sorting and accessible alternatives to dense mobile tables.
- Keep Metronic-specific styling and dependencies isolated from the public design system.

### 8.4 Frontend testing

- Unit/component tests for behavior and state transitions.
- Contract-backed API tests with deterministic mocks; a small set of browser end-to-end tests for discovery, contact, login, authoring, upload, publish, and public verification.
- Automated lint/type/build/accessibility checks, visual regression for core responsive screens, and performance budgets for public routes.

## 9. ASP.NET Core Architecture

### 9.1 Feature/use-case design

Each feature exposes cohesive commands/queries such as `ListProjects`, `GetProjectBySlug`, `PublishInfographic`, or `SubmitContactMessage`. Endpoints bind transport data and delegate. Handlers validate, authorize, apply domain rules, invoke ports, persist atomically, emit audited/analytics events, and return explicit result types. Public query paths use DTO projections and `AsNoTracking`.

### 9.2 API standards

- Separate public and `/api/admin` surfaces. Keep routes and contracts compatible with future versioning, but add explicit versions only when multiple client versions or external consumers require them.
- OpenAPI documents success and error contracts. Validation failures, conflict, not found, unauthorized, forbidden, throttled, and unexpected errors use consistent problem details without leaking internals.
- Require server-side pagination with bounded page size; whitelist sorting/filter fields; normalize slugs and search safely.
- Use optimistic concurrency for admin edits and explicit publish state transitions (draft, scheduled if approved, published, archived).
- Keep MVP engagement data to approved essential counters. Any later view/download/share/helpful/rating events require defined consent/privacy, retention, bot, and deduplication rules.

### 9.3 Infrastructure and security

- EF Core repositories/query services exist only where they clarify use cases; avoid generic repositories over `DbContext`.
- Abstract object storage, email, clock, current user, and external analytics. Use background processing/outbox semantics for work that must survive request failure (email, media processing, event aggregation) if reliability requirements warrant it.
- Use ASP.NET Core Identity or an equivalently reviewed implementation; never design password hashing/token security ad hoc. Store token hashes, rotate refresh tokens, revoke reuse, and rate-limit sensitive endpoints.
- Validate upload signature/type/size/dimensions, randomize storage keys, prevent executable public content, and define orphan cleanup. Malware scanning is an Enterprise Hardening enhancement.
- Use least-privilege managed identities/service principals, Key Vault, encrypted transport/storage, database firewall/private networking where available, and dependency/container scanning.

### 9.4 Backend testing

- Domain/unit tests for rules and handlers; architecture tests for dependency boundaries.
- Integration tests against SQL Server semantics—not only an in-memory provider—for mappings, queries, migrations, authorization, storage adapters, and endpoint contracts.
- End-to-end smoke tests in staging plus targeted performance tests for homepage composition, content listings, and media delivery paths.

## 10. Vertical Slice Strategy and Implementation Order

Every feature is implemented as a complete vertical slice containing **database changes, API behavior, Angular UI, testing, and documentation**. A slice must leave the application in a working state. Database-only, API-only, or page-only feature delivery is not complete. Typical branches include `feature/homepage`, `feature/projects`, `feature/visual-handbook`, and `feature/admin-content`.

1. **Platform foundation:** solution, CI, local environment, health/config/logging, error contracts, design tokens, API client workflow.
2. **Persistence and admin authentication:** reconciled core schema, migrations, single-admin bootstrap, login/refresh/logout, audit baseline, admin shell.
3. **Public foundation:** responsive shell, navigation, footer, theme, shared accessible components, SEO primitives, and common UI states.
4. **Homepage:** approved profile and featured-content composition, responsive homepage, API/data integration, SEO, tests, and documentation.
5. **Projects:** taxonomy/technologies/media dependencies, public list/details, and basic admin create/edit/preview/publish.
6. **Visual Handbook:** required categories/tags, public list/filter/details/viewer, media upload, and basic admin authoring/publishing.
7. **About:** approved profile, skills, certifications, and settings-backed content.
8. **Experience:** approved career history and CV metadata/download if authorized.
9. **Contact:** abuse-resistant submission, delivery, privacy/retention controls, and basic admin message handling if required.
10. **Admin content completion:** cohesive single-admin CMS, authentication, publishing workflows, and media upload across MVP content.
11. **MVP hardening and deployment:** SEO completion, security baseline, accessibility, performance, browser/device QA, backup, essential monitoring, staging UAT, and launch.
12. **Future enhancements:** Learning Paths, Articles, Notifications, advanced analytics, recommendation engine, multi-user administration, advanced roles, media collections, bookmarks, community features, global search, and richer Series experiences only after separate approval.

Projects precede the Visual Handbook to align with the supplied roadmap; both remain equal product pillars in navigation and homepage prominence.

## 11. Deployment Strategy

### 11.1 Environments and infrastructure

- Maintain isolated **development**, **staging**, and **production** environments with separate databases, storage, secrets, domains, analytics properties, and retention policies.
- Prefer infrastructure as code (Bicep or Terraform; decision required). A pragmatic Azure topology is static/SSR-capable hosting for `Portfolio.Web`, static hosting for `Portfolio.Admin`, Azure App Service or Azure Container Apps for `Portfolio.Api`, Azure SQL Database, Azure Blob Storage plus CDN/Front Door, Key Vault, and Application Insights/Azure Monitor.
- Choose public Angular hosting only after the SSR decision; do not force an SSR build onto static-only hosting.
- Configure DNS/TLS, application rate limiting, CORS/CSP, custom error pages, database access controls, budgets, and environment tags through IaC.

### 11.2 CI/CD flow

1. Pull requests: restore with locked dependencies; format/lint/type-check; unit, integration, architecture, and migration checks; build all deployables; scan dependencies/secrets; publish review artifacts.
2. Merge to `dev`: produce immutable versioned artifacts/images, deploy automatically to development, run smoke and contract tests.
3. Release candidate: promote the exact artifact to staging, apply controlled database migration, run E2E/accessibility/performance/security checks, and obtain UAT approval.
4. Merge to `main` and tag: require production approval, back up data, apply backward-compatible migrations, deploy the approved artifact, and verify health, smoke tests, and essential telemetry.
5. Rollback: restore the previous application artifact immediately when compatible; prefer a forward database fix and use a tested restore procedure only when necessary. Never assume destructive migrations are trivially reversible.

### 11.3 Operations

For the MVP, monitor availability, error rate, authentication anomalies, contact/email failures, storage/database capacity, and Core Web Vitals. Use essential alerts, retention-aware logs, automated backups, dependency patching, an application rollback runbook, and post-release review.

### 11.4 Optional Enterprise Hardening

The following capabilities are valuable future enterprise enhancements, not MVP release requirements:

- distributed tracing across clients, API, database, storage, and background processing;
- Web Application Firewall policies and advanced edge protection;
- blue/green or canary deployment with automated traffic shifting;
- formal disaster-recovery drills and cross-region recovery;
- malware scanning and quarantine workflows for uploaded media;
- advanced operational monitoring, service-level objectives, and anomaly detection; and
- full Recovery Point Objective (RPO) and Recovery Time Objective (RTO) planning.

Adopt these controls when traffic, risk, compliance, availability objectives, or organizational operations justify their cost. Their future introduction should not require redesigning the core application boundaries.

## 12. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Source files have generated names and the requested canonical names are absent. | Misidentification and broken references. | Confirm mapping, then rename/move only in an approved documentation change. |
| ERD scope conflicts with explicit product non-goals (learning paths, article/polymorphic entities). | Overbuilt MVP and confusing positioning. | Reconcile and approve an MVP data dictionary before entity generation. |
| Homepage reference contains unverified/sample statistics and content. | Incorrect public claims or confidentiality exposure. | Bind only owner-approved facts/CMS counts; perform content/privacy review. |
| SSR/prerendering remains undecided. | Hosting rework and weak SEO if delayed. | Prototype dynamic routes and choose rendering/hosting in Milestone 0. |
| Metronic version/license/integration constraints are unknown. | Legal, upgrade, bundle-size, and styling risk. | Verify license and supported Angular version before admin scaffolding. |
| Broad initial schema and admin surface. | Slow delivery and large migrations. | Enforce slice order; defer multi-user and optional modules. |
| Analytics duplication, bot traffic, or privacy ambiguity. | Misleading reports and compliance risk. | Define event semantics, consent, deduplication, retention, and reconciliation first. |
| Media uploads and high-resolution viewing. | Security, cost, bandwidth, accessibility, and performance issues. | Validate file signature/type/size, block executable content, transform, CDN-cache, set quotas, preserve approved originals, and test viewer UX; consider malware scanning during Enterprise Hardening. |
| Authentication/token design shown in ERD may not match deployment threat model. | Account compromise. | Threat-model first; use framework identity, secure storage, rotation/revocation, MFA-ready design, and audit. |
| Confidential government/defence project material. | Professional, contractual, or security harm. | Approval workflow, safe descriptions, sanitized media, and pre-publication checklist. |
| Single-administrator operation. | Lockout and continuity risk. | Secure recovery/bootstrap runbook, backups, MFA decision, and break-glass procedure. |
| Separate client deployments can drift from the API. | Runtime incompatibility. | Versioned contracts, generated clients, contract tests, and coordinated artifact promotion. |

## 13. Open Questions

Questions marked **blocking** should be resolved before the relevant milestone begins.

1. **Blocking—sources:** Do the generated filenames map to the five canonical names exactly as recorded at the top of this plan, and may they later be renamed/moved into `/docs`?
2. **Blocking—database specification:** When will `Database_Specification.md` be finalized, and who approves reconciliation between its written rules and the ERD?
3. **Blocking—ERD reconciliation:** Which schema owns Profile, Experience Items, Skills, Certifications, Project Links, and soft-delete/audit fields that the master document requires but the pictured model does not clearly cover?
4. **Blocking—design:** Are the complete Figma screens/design-system/handoff files available and approved, or is the supplied homepage image the only approved screen?
5. Which homepage claims, project names/images, counts, CV file, external links, and contact details are approved for publication? Should Login be visible in public navigation?
6. Which supported Angular, .NET, EF Core, SQL Server/Azure SQL, Tailwind, and Metronic versions should be pinned at kickoff, and is the Metronic license available?
7. Should public routes use Angular SSR, prerendering, or a hybrid? How frequently will published content change, and what cache freshness is acceptable?
8. Is Azure the mandated cloud? Choose App Service versus Container Apps, IaC tool, region, domains, DNS owner, budget, and availability/recovery targets.
9. Choose Azure Blob Storage versus Cloudinary; define maximum types/sizes/dimensions, transformations, original-download policy, retention, and CDN behavior. Decide separately whether future Enterprise Hardening requires malware scanning.
10. Is admin authentication local, Microsoft Entra ID, or another identity provider? Are MFA and account recovery launch requirements? Multi-user and fine-grained role administration are future enhancements.
11. Which email provider/sender/domain will handle contact notifications, and what CAPTCHA/honeypot, consent, deletion, and retention requirements apply?
12. Choose Clarity or GA4 (or neither) for basic MVP measurement and define cookie/consent requirements. Advanced internal analytics, bot filtering, and event reconciliation are future scope.
13. What are the measurable browser, accessibility (recommended WCAG 2.2 AA), SEO/Core Web Vitals, performance, and uptime acceptance targets? Full RPO/RTO planning belongs to optional Enterprise Hardening.
14. Is bilingual/RTL content (Arabic/English) required now or later? The answer affects schema, URLs, layouts, search, and content workflows.
15. Who reviews and approves releases, confidential case-study content, database migrations, and production access?

## 14. Assumptions

Until the open questions are answered, planning assumes:

- The current generated files correspond to the five named source artifacts as mapped in this plan; no source asset is changed by this deliverable.
- English and left-to-right layout are MVP; localization is not silently precluded but is not implemented speculatively.
- The first release has one administrator. Users/Roles UI and fine-grained permission management are post-MVP, while secure admin authentication remains mandatory.
- Projects and the Visual Handbook have equal strategic prominence; projects are implemented first only to control delivery dependencies.
- Only approved CV facts and confidentiality-safe case studies are published. Counts come from authoritative data or approved copy, never from the reference mockup alone.
- SQL Server/Azure SQL stores structured data; object storage stores media bytes. Azure is the provisional deployment target, not yet a binding vendor decision.
- Public routes require strong SEO; SSR/hybrid rendering is the provisional recommendation pending a spike.
- `Portfolio.Web`, `Portfolio.Admin`, and `Portfolio.Api` remain independently deployable. API contracts are tested and designed to permit future versioning without requiring explicit versioning in the MVP.
- WCAG 2.2 AA, responsive desktop/tablet/mobile behavior, secure defaults, privacy-aware basic measurement, and essential production monitoring are MVP release requirements.
- Managed content supports draft/published/archived states; scheduling is included only if confirmed. High-risk destructive changes require audit and confirmation.
- Learning Paths, Articles, Notifications, advanced analytics, recommendations, multi-user administration, advanced roles, media collections, bookmarks, and community features are outside MVP.

## Concise Implementation Roadmap

1. **Confirm scope:** approve source mapping, Figma handoff, content facts, MVP entities, non-functional targets, cloud/provider decisions, and ADRs.
2. **Establish repository:** create the protected `main`/`dev` workflow when authorized; add solution/workspaces, standards, local dependencies, CI, security scans, and documentation structure.
3. **Build foundations:** implement API/Angular shells, OpenAPI client flow, authentication, telemetry, health checks, design tokens, and a reconciled EF Core model with tested migrations.
4. **Deliver public value:** ship the responsive Homepage, Projects, Visual Handbook, About, Experience, Contact, SEO, accessibility, and required UI states as end-to-end slices.
5. **Enable ownership:** deliver secure single-admin authentication, basic content authoring/publishing, and media upload.
6. **Harden the MVP:** complete contract/integration/E2E, confidentiality, security-baseline, accessibility, performance, browser/device, migration, backup, and essential operational checks in staging.
7. **Release:** provision isolated environments through IaC, promote immutable artifacts through development and staging, approve UAT, migrate safely, deploy production, verify, monitor, and retain application rollback/forward-fix readiness.
8. **Iterate:** prioritize future Learning Paths, Articles, Notifications, advanced analytics, recommendations, multi-user/role administration, media collections, bookmarks, community capabilities, and optional Enterprise Hardening through separately approved milestones.
