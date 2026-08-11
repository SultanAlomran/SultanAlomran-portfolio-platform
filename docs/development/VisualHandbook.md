# Visual Handbook / Infographics

## Terminology and scope

- **Visual Handbook** is the public product section.
- **Infographic** is the persisted technical guide displayed in that section.
- **Series** is an optional ordered association already represented by `SeriesItem`; complete management and Reading Paths remain Issue #39.
- **Media Library** owns future upload, replacement, and provider administration. This slice only selects existing `MediaFile` metadata; Issue #49 remains the full workflow.

No `VisualHandbook` table or duplicate content domain was introduced.

## Persisted model

The existing `Infographic`, `InfographicStep`, `InfographicResource`, `InfographicCodeExample`, `InfographicTag`, `Category`, `Tag`, `SeriesItem`, and `MediaFile` entities are reused. The `InfographicMediaAndFeatured` migration adds:

- `IsFeatured`;
- optional cover-image, main-infographic-image, and PDF `MediaFile` references;
- restrictive delete behavior and dashboard/list indexes.

SQL Server stores content and media references only. Image and PDF binaries remain in the configured media provider and are never stored in these rows.

## Routes

Public UI:

- `/visual-handbook`
- `/visual-handbook/:slug`

Admin UI:

- `/infographics`
- `/infographics/create`
- `/infographics/:id`
- `/infographics/:id/edit`

Public API:

- `GET /api/infographics`
- `GET /api/infographics/featured`
- `GET /api/infographics/{slug}`
- taxonomy lookups under `/api/infographics/taxonomy`

Admin API supports list/details/create/update/delete, draft, publish readiness, publish, archive, category/tag lookups, and selectable media under `/api/admin/infographics`. Development OpenAPI and Scalar expose these endpoints.

## Development seed

The seed is explicit, LocalDB-only, deterministic, and idempotent:

```powershell
dotnet run --project src/Portfolio.Api -- --seed-development-infographics
```

It creates a focused set of published and draft engineering guides across .NET, Angular, SQL/Data, Architecture, APIs/Integration, and OutSystems. It does not create engagement metrics or binary media.

## Current boundaries

- Authentication/authorization is still deferred under the platform-wide policy; Admin mutation routes must be protected before production exposure.
- Media can be selected from existing metadata, but upload and storage-provider administration remain Issue #49.
- Existing Series associations are readable; complete Series Admin, ordering, and Reading Paths remain Issue #39.
- Missing optional images/PDFs use deliberate fallback states and never render broken actions.
