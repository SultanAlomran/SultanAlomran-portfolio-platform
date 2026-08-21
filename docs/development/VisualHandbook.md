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

## Browser-local engagement and privacy

Issue #38 adds a privacy-first continuation experience without creating a public identity:

- bookmarks are stored under `portfolio.visualHandbook.bookmarks.v1` and are bounded to 50 minimal guide references;
- recently viewed history is stored under `portfolio.visualHandbook.recentlyViewed.v1` and is bounded to 12 entries;
- furthest reading progress is stored under `portfolio.visualHandbook.readingProgress.v1` and is bounded to 20 entries;
- storage contains only guide ID, slug, title, timestamps, and progress percentage—never content bodies, credentials, or sensitive data;
- malformed, unavailable, or quota-limited storage falls back safely without blocking public content;
- no browser/device fingerprint, IP identity, advertising tracker, public account, or SQL synchronization is introduced.

The Saved filter resolves only current published guides through a bounded read-only `GET /api/infographics/by-ids` request. The API does not persist the visitor's saved list. Related Guides are derived deterministically from real Series, category, and tag metadata. Previous/Next links are emitted only for published neighbors in an existing ordered Series. Share actions use the browser Share/Clipboard APIs and a plain LinkedIn URL; download actions continue to reference existing media metadata.

The existing `UserHelpfulVote`, `UserRating`, and `UserBookmark` tables remain unchanged. They require a real `Users.Id`, and their unique identity/content indexes and rating check constraint remain authoritative. The only browser identity currently implemented is the private Administrator cookie; analytics `Session` rows are not public authentication. Helpful/Not Helpful, structured negative reasons, and rating writes therefore remain blocked pending an approved public identity, consent, retention, and abuse/deduplication design. Administrator authentication is deliberately not reused for public engagement. No EF migration is required by this slice.

## Current boundaries

- Private Administrator authentication protects Admin mutations; no public visitor authentication or account system exists.
- Media can be selected from existing metadata, but upload and storage-provider administration remain Issue #49.
- Existing Series associations are readable; complete Series Admin, ordering, and Reading Paths remain Issue #39.
- Missing optional images/PDFs use deliberate fallback states and never render broken actions.
