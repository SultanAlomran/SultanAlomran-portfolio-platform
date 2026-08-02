# Sultan Alomran Portfolio Platform — Implementation Plan

**Status:** Planning baseline; no application implementation or scaffolding is authorized by this document.  
**Prepared:** 2 August 2026  
**Source-of-truth inputs reviewed:** the repository's Project 00 master document (`Project_00_Master_Document_to_Give_Figma_AI 2.md`), ERD (`D1036970-646E-4B76-89A1-E280BDA8A0E8.png`), homepage reference (`IMG_0266.jpeg`), roadmap (`AB5208DD-3E05-4E0F-8D04-BF8DC1F92E51.png`), and branching workflow (`C80D222F-58C0-4BB7-9B10-BEBD761CF5C0.png`). These are the files currently present under generated names and correspond to the five artifacts named in the project brief.

## 1. Executive Summary

The product is a premium, content-managed engineering portfolio for Sultan Alomran. It has two equally important public pillars—enterprise **Projects / Case Studies** and a **Visual Handbook / Technical Content** library—supported by professional profile, experience, certifications, search, contact, and measured engagement. It is explicitly not a course platform, social network, generic blog, or generic dashboard.

The target solution consists of three separately deployable applications:

- **Portfolio.Web:** a custom, SEO-ready public Angular application using Tailwind CSS.
- **Portfolio.Admin:** a private Angular CMS using Metronic conventions.
- **Portfolio.Api:** an ASP.NET Core Web API using EF Core and SQL Server, with authentication, media integration, analytics, and content-management capabilities.

Implementation should proceed in complete, testable vertical slices after design/handoff artifacts are approved. The initial production release should prioritize the public discovery journeys, single-administrator content operations, media, messages, analytics, accessibility, SEO, security, and operational readiness. Multi-user role management, optional learning paths, articles, and advanced media organization should remain deferred until their scope is confirmed.

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
- Use REST conventions, explicit error contracts, server-side pagination/filtering, cancellation, idempotency where needed, and API versioning from the first public contract.
- Treat accessibility, privacy, SEO, performance, observability, and secure defaults as acceptance criteria rather than later polish.
- Separate environments and configuration; keep secrets outside source control.

### 2.3 Cross-cutting capabilities

- Authentication/authorization for the admin surface, short-lived access plus securely rotated refresh credentials (or secure cookie-based browser authentication after threat-model review), rate limiting, and audit trails.
- Central validation, RFC 9457-style problem responses, exception mapping, structured logs, correlation/trace IDs, health/readiness checks, metrics, and distributed tracing.
- Output caching for safe public reads; cache invalidation after publishing; CDN delivery and optimized variants for public media.
- Content security policy, strict CORS allowlists, anti-forgery protection when cookies are used, secure headers, input sanitization, upload validation/malware controls, and contact-form abuse protection.
- OpenAPI as the API contract source; generate typed Angular clients only after contract review.

## 3. Repository Structure

### 3.1 Recommended target organization

```text
/
├── README.md
├── LICENSE                         # if/when selected
├── .editorconfig
├── .gitignore
├── global.json                     # pinned supported .NET SDK
├── Directory.Build.props
├── Directory.Packages.props
├── package.json                    # optional workspace-level commands only
├── Portfolio.sln
├── apps/
│   ├── portfolio-web/              # Portfolio.Web Angular workspace/project
│   ├── portfolio-admin/            # Portfolio.Admin Angular workspace/project
│   └── portfolio-api/              # ASP.NET Core host
├── src/                            # .NET non-host projects, grouped by boundary
├── tests/                          # backend architecture/unit/integration tests
├── deploy/                         # IaC, deployment manifests, runbooks
├── docs/                           # approved source artifacts and ADRs
├── scripts/                        # repeatable local/CI automation
└── .github/workflows/              # validation and deployment pipelines
```

This is a proposed end state, not an instruction to scaffold now. Keep the public and admin Angular applications independently buildable/deployable even if workspace tooling is later shared.

### 3.2 Current organization review

The repository currently contains planning/reference assets at its root plus a short README; no application structure exists. That is appropriate at the planning stage, but generated asset names make their intent difficult to discover. Before solution scaffolding, it is appropriate to **recommend** a `/docs` folder containing clearly named source artifacts and an index, for example `docs/source-of-truth/Project_00_Master_Document.md`, `ERD.png`, `Homepage_Reference.png`, `Roadmap.png`, and `Workflow.png`, plus `docs/adr/`. Do **not** move or rename them until the owner approves, because existing external references may rely on their current paths. `Implementation_Plan.md` should remain at the root unless the owner requests otherwise.

## 4. Folder Structure

### 4.1 ASP.NET Core internals

```text
apps/portfolio-api/
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
├── Portfolio.Domain/              # entities/value objects/domain rules
├── Portfolio.Application/         # use cases and ports, if extraction is justified
└── Portfolio.Infrastructure/      # adapters, if extraction is justified
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

Public features are Home, Projects, Visual Handbook, Infographic Details, Series, Experience, About, Contact, Search, and Not Found. Admin features are Auth, Dashboard, Infographics, Projects, Taxonomy, Series, Media, Analytics, Messages, Settings, and—later—Users/Roles. Sharing between the two applications should initially be limited to generated API contracts and intentionally packaged design-agnostic utilities; their visual systems are different.

## 5. Development Milestones

| Milestone | Outcome | Exit criteria |
|---|---|---|
| **0. Scope and design readiness** | Resolve contradictions, approve responsive designs, component states, content inventory, API/data decisions, and non-functional targets. | Open questions answered; designs and technical handoff approved; privacy review complete. |
| **1. Repository and solution foundation** | Buildable solution, two Angular applications, API host, formatting/linting, tests, OpenAPI, logging, configuration, health checks, local dependencies, CI. | Clean clone can build/test deterministically; no production secrets; architecture checks pass. |
| **2. Persistence and platform foundation** | Reconciled model, EF configurations/migrations, seed strategy, storage abstraction, authentication baseline, telemetry. | Reviewed initial migration works on a fresh database and upgrade path; rollback/restore rehearsal documented. |
| **3. Public foundation and homepage** | Design tokens, responsive shell, navigation/footer/theme, home API composition, homepage, SEO metadata, loading/error/empty states. | Approved desktop/tablet/mobile parity; accessibility and performance budgets pass. |
| **4. Projects vertical slice** | Listing, filters/pagination, details/case studies, links/images/technologies, admin CRUD and publish workflow. | End-to-end author-to-public flow tested; confidentiality checks enforced. |
| **5. Visual Handbook vertical slice** | Categories/tags, listing/search/filtering, infographic viewer, download/share/engagement, admin editing/bulk media. | Viewer is accessible and responsive; file validation and analytics semantics verified. |
| **6. Series and discovery** | Ordered series, related content, global search, shareable filter state. | Ordering and search relevance tests pass; canonical/indexing rules verified. |
| **7. Profile and communication** | Experience, About, certifications/skills, Contact, CV workflow, admin messages/settings. | Approved facts only; spam/data-retention controls and email delivery verified. |
| **8. Admin and analytics completion** | Dashboard, media library, publishing UX, audit trail, purposeful internal analytics. | Authorization matrix and event definitions tested; operational views reconcile with event data. |
| **9. Hardening and release** | Security, accessibility, performance, cross-browser/responsive QA, backup/restore, monitoring, runbooks, staging UAT. | Release checklist signed; critical issues closed; tested rollback and disaster-recovery procedure. |
| **10. Deferred capabilities** | Multi-user roles/permissions, optional learning paths/articles/media collections, only after validation. | Separate approved scope and migrations; no effect on MVP release. |

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

Expected feature sequence follows the workflow reference: solution foundation, persistence foundation, public foundation, homepage, projects, Visual Handbook, about, experience, contact, admin foundation/content, analytics, and deployment. Slice names may be split when a pull request becomes too large; completeness matters more than mirroring a diagram literally. **No branch is to be created as part of this planning deliverable.**

## 7. Database Generation Strategy

### 7.1 Reconcile before generating

The ERD is a detailed logical starting point, not a license to generate unchecked code. First reconcile it with the master document. The ERD includes `LearningPaths`, `LearningPathItems`, code examples/resources/steps, bookmarks, user interactions, articles as polymorphic types, and several token/session models that are either optional, deferred, or potentially inconsistent with the explicit non-goal of becoming a learning platform. Conversely, the master document calls for profile, experience, skills, certifications, project links, and audit behavior that need confirmation against the pictured schema.

Produce an approved data dictionary containing ownership, nullability, lengths, enum values, defaults, uniqueness, delete behavior, privacy class, retention, indexes, and audit rules before writing entities or a migration.

### 7.2 EF Core approach

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

- Separate public and `/api/admin` surfaces and introduce a consistent version policy before clients depend on them.
- OpenAPI documents success and error contracts. Validation failures, conflict, not found, unauthorized, forbidden, throttled, and unexpected errors use consistent problem details without leaking internals.
- Require server-side pagination with bounded page size; whitelist sorting/filter fields; normalize slugs and search safely.
- Use optimistic concurrency for admin edits and explicit publish state transitions (draft, scheduled if approved, published, archived).
- Record view/download/share/helpful/rating events with clear deduplication, consent/privacy, retention, and bot rules; never inflate authoritative content counters directly from unauthenticated input.

### 9.3 Infrastructure and security

- EF Core repositories/query services exist only where they clarify use cases; avoid generic repositories over `DbContext`.
- Abstract object storage, email, clock, current user, and external analytics. Use background processing/outbox semantics for work that must survive request failure (email, media processing, event aggregation) if reliability requirements warrant it.
- Use ASP.NET Core Identity or an equivalently reviewed implementation; never design password hashing/token security ad hoc. Store token hashes, rotate refresh tokens, revoke reuse, and rate-limit sensitive endpoints.
- Validate upload signature/type/size/dimensions, randomize storage keys, prevent executable public content, scan files where feasible, and define orphan cleanup.
- Use least-privilege managed identities/service principals, Key Vault, encrypted transport/storage, database firewall/private networking where available, and dependency/container scanning.

### 9.4 Backend testing

- Domain/unit tests for rules and handlers; architecture tests for dependency boundaries.
- Integration tests against SQL Server semantics—not only an in-memory provider—for mappings, queries, migrations, authorization, storage adapters, and endpoint contracts.
- End-to-end smoke tests in staging plus load tests for homepage composition, search/listing, media delivery paths, and event ingestion.

## 10. Vertical Slice Implementation Order

Each slice includes schema/migration (when needed), API contract and use case, both relevant Angular surfaces, automated tests, analytics/telemetry, accessibility, documentation, and deployment verification.

1. **Platform foundation:** solution, CI, local environment, health/config/logging, error contracts, design tokens, API client workflow.
2. **Persistence and admin authentication:** reconciled core schema, migrations, single-admin bootstrap, login/refresh/logout, audit baseline, admin shell.
3. **Public shell and homepage:** profile statistics/featured content composition, responsive home, navigation/footer/theme, SEO and public states. Start with approved static seed content if CMS authoring is not yet present.
4. **Projects:** taxonomy/technologies/media dependencies, public list/details/related content, admin create/edit/preview/publish.
5. **Visual Handbook:** categories/tags, list/filter/search, details/viewer/download/share/helpful/rating, admin editor and bulk upload.
6. **Series:** ordered membership, public series details/progress context, admin reorder/publish.
7. **Global discovery:** unified search, persistent filters, related-content policy, 404 recovery.
8. **Profile:** Experience, About, skills/technologies, certifications, CV metadata/download, settings-backed approved facts.
9. **Contact and messages:** abuse-resistant submission, email notification, admin inbox/status/archive, privacy/retention controls.
10. **Media management:** reusable library, usage references, replace/delete protection, processing and cleanup. Basic upload lands earlier as needed; this slice completes operations.
11. **Analytics and dashboard:** authoritative event vocabulary, aggregation, dashboard/content reports, sources/search terms, reconciliation and retention.
12. **Release hardening/deployment:** full QA, security/performance/accessibility, backup/restore, monitoring, production rehearsal and launch.
13. **Post-MVP:** Users/Roles, learning paths, articles, code examples/resources/steps, bookmarks, notifications, and advanced media collections only after approval.

Projects precede the Visual Handbook to align with the supplied roadmap; both remain equal product pillars in navigation and homepage prominence.

## 11. Deployment Strategy

### 11.1 Environments and infrastructure

- Maintain isolated **development**, **staging**, and **production** environments with separate databases, storage, secrets, domains, analytics properties, and retention policies.
- Prefer infrastructure as code (Bicep or Terraform; decision required). A pragmatic Azure topology is static/SSR-capable hosting for `Portfolio.Web`, static hosting for `Portfolio.Admin`, Azure App Service or Azure Container Apps for `Portfolio.Api`, Azure SQL Database, Azure Blob Storage plus CDN/Front Door, Key Vault, and Application Insights/Azure Monitor.
- Choose public Angular hosting only after the SSR decision; do not force an SSR build onto static-only hosting.
- Configure DNS/TLS, WAF/rate limiting where justified, CORS/CSP, custom error pages, database firewall/private access, budgets, and environment tags through IaC.

### 11.2 CI/CD flow

1. Pull requests: restore with locked dependencies; format/lint/type-check; unit, integration, architecture, and migration checks; build all deployables; scan dependencies/secrets; publish review artifacts.
2. Merge to `dev`: produce immutable versioned artifacts/images, deploy automatically to development, run smoke and contract tests.
3. Release candidate: promote the exact artifact to staging, apply controlled database migration, run E2E/accessibility/performance/security checks, and obtain UAT approval.
4. Merge to `main` and tag: require production approval, backup, apply backward-compatible migrations, use rolling/blue-green deployment, verify health/smoke/telemetry, then expose traffic.
5. Rollback: restore the previous application artifact immediately when compatible; prefer a forward database fix and use a tested restore procedure only when necessary. Never assume destructive migrations are trivially reversible.

### 11.3 Operations

Monitor availability, latency, error rate, dependency failures, auth anomalies, contact/email failures, storage/database capacity, job backlog, and Core Web Vitals. Use actionable alerts, dashboards, structured runbooks, retention-aware logs, automated backups with point-in-time recovery, periodic restore drills, dependency patching, and post-release review.

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
| Media uploads and high-resolution viewing. | Security, cost, bandwidth, accessibility, and performance issues. | Validate/scan, transform, CDN-cache, set quotas, preserve downloadable originals, and test viewer UX. |
| Authentication/token design shown in ERD may not match deployment threat model. | Account compromise. | Threat-model first; use framework identity, secure storage, rotation/revocation, MFA-ready design, and audit. |
| Confidential government/defence project material. | Professional, contractual, or security harm. | Approval workflow, safe descriptions, sanitized media, and pre-publication checklist. |
| Single-administrator operation. | Lockout and continuity risk. | Secure recovery/bootstrap runbook, backups, MFA decision, and break-glass procedure. |
| Separate client deployments can drift from the API. | Runtime incompatibility. | Versioned contracts, generated clients, contract tests, and coordinated artifact promotion. |

## 13. Open Questions

Questions marked **blocking** should be resolved before the relevant milestone begins.

1. **Blocking—sources:** Do the generated filenames map to the five canonical names exactly as recorded at the top of this plan, and may they later be renamed/moved into `/docs`?
2. **Blocking—data scope:** Are Learning Paths, Learning Path Items, Infographic Steps/Resources/Code Examples, Articles, Bookmarks, Notifications, Media Collections, and general `UserInteractions` in MVP, post-MVP, or obsolete ERD concepts?
3. **Blocking—ERD reconciliation:** Which schema owns Profile, Experience Items, Skills, Certifications, Project Links, and soft-delete/audit fields that the master document requires but the pictured model does not clearly cover?
4. **Blocking—design:** Are the complete Figma screens/design-system/handoff files available and approved, or is the supplied homepage image the only approved screen?
5. Which homepage claims, project names/images, counts, CV file, external links, and contact details are approved for publication? Should Login be visible in public navigation?
6. Which supported Angular, .NET, EF Core, SQL Server/Azure SQL, Tailwind, and Metronic versions should be pinned at kickoff, and is the Metronic license available?
7. Should public routes use Angular SSR, prerendering, or a hybrid? How frequently will published content change, and what cache freshness is acceptable?
8. Is Azure the mandated cloud? Choose App Service versus Container Apps, IaC tool, region, domains, DNS owner, budget, and availability/recovery targets.
9. Choose Azure Blob Storage versus Cloudinary; define maximum types/sizes/dimensions, transformations, original-download policy, malware scanning, retention, and CDN behavior.
10. Is admin authentication local, Microsoft Entra ID, or another identity provider? Are MFA, account recovery, multiple administrators, and fine-grained permissions launch requirements?
11. Which email provider/sender/domain will handle contact notifications, and what CAPTCHA/honeypot, consent, deletion, and retention requirements apply?
12. Choose Clarity or GA4 (or neither), cookie/consent requirements, internal analytics event definitions, bot filtering, deduplication, and retention.
13. What are the measurable browser, accessibility (recommended WCAG 2.2 AA), SEO/Core Web Vitals, performance, uptime, RPO, and RTO acceptance targets?
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
- `Portfolio.Web`, `Portfolio.Admin`, and `Portfolio.Api` remain independently deployable. API contracts are versioned and tested.
- WCAG 2.2 AA, responsive desktop/tablet/mobile behavior, secure defaults, privacy-aware analytics, and production observability are release requirements.
- Managed content supports draft/published/archived states; scheduling is included only if confirmed. High-risk destructive changes require audit and confirmation.
- Learning/course-like capabilities, public comments, pricing, subscriptions, and community features are outside MVP.

## Concise Implementation Roadmap

1. **Confirm scope:** approve source mapping, Figma handoff, content facts, MVP entities, non-functional targets, cloud/provider decisions, and ADRs.
2. **Establish repository:** create the protected `main`/`dev` workflow when authorized; add solution/workspaces, standards, local dependencies, CI, security scans, and documentation structure.
3. **Build foundations:** implement API/Angular shells, OpenAPI client flow, authentication, telemetry, health checks, design tokens, and a reconciled EF Core model with tested migrations.
4. **Deliver public value:** ship responsive homepage, Projects, Visual Handbook, Series, Search, Experience/About, Contact, SEO, accessibility, and all required states as end-to-end slices.
5. **Enable ownership:** deliver single-admin content authoring, publishing/preview, media, messages, settings, audit, and purposeful analytics.
6. **Harden:** complete contract/integration/E2E, confidentiality, security, accessibility, performance, browser/device, migration, backup/restore, and operational testing in staging.
7. **Release:** provision isolated Azure environments through IaC, promote immutable artifacts through development and staging, approve UAT, migrate safely, deploy production, verify, monitor, and retain rollback/forward-fix readiness.
8. **Iterate:** use validated engagement and operational feedback to prioritize post-MVP roles, learning paths, articles, advanced media, and other explicitly approved capabilities.
