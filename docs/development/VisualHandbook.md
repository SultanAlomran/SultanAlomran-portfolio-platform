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
- `GET /api/infographics/by-ids`
- `GET /api/infographics/{slug}/engagement`
- `PUT /api/infographics/{id}/helpful-vote`
- `PUT /api/infographics/{id}/rating`
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

The Saved filter resolves only current published guides through a bounded read-only `GET /api/infographics/by-ids` request. The API does not persist the visitor's saved list. Related Guides are derived deterministically from real Series, category, and tag metadata. Previous/Next links are emitted only for published neighbors in an existing ordered Series. Share actions use native Share/Clipboard APIs. The LinkedIn action opens LinkedIn's official share-offsite URL and copies a guide-specific suggested caption for the visitor to paste. Per-guide Open Graph title, description, canonical URL, and cover-or-main-image metadata are set in the page. Direct LinkedIn posting or image upload is not performed because it requires LinkedIn OAuth consent and `w_member_social`; the static SPA still needs a future SSR/dynamic-rendering layer for guaranteed crawler-visible per-guide image previews. Download actions continue to reference existing media metadata.

## Server-persisted feedback and anonymous identity

`UserHelpfulVote` and `UserRating` are reused; no duplicate engagement or anonymous-user table is created. Each row belongs to exactly one actor type: an authenticated `UserId` or an anonymous `VisitorKeyHash`. SQL check constraints enforce that exclusivity, and filtered unique indexes enforce one vote and one rating per actor/content pair. Existing authenticated records remain valid.

The first Helpful or Rating write creates a 32-byte random first-party token in a 180-day, HttpOnly, SameSite=Lax cookie scoped to `/api/infographics` (Secure outside Development). The raw token never enters SQL or logs; SQL receives only its SHA-256 hash. Reads do not create the cookie. There is no public account, Administrator coupling, fingerprint, stored IP address, country lookup, user-agent identity, advertising tracker, or anonymous user record.

Helpful votes are upserted and may change direction. Not Helpful can store one optional `NegativeFeedbackReason` value; changing to Helpful clears it. Ratings are upserted and retain the database `1..5` check. Aggregate queries stay server-side and return raw Helpful/Not Helpful counts, percentage, average, count, a five-bucket distribution, and structured-reason counts. Write endpoints allow 20 operations per five minutes, partitioned by the anonymous token or transient remote IP before a token exists; the IP partition is in memory and is never persisted.

Clearing the cookie or waiting for it to expire creates a new browser identity and can permit another response. Preventing that without login would require stronger tracking or fingerprinting, which this design deliberately rejects. Engagement rows currently follow the content/database retention lifecycle; a future Analytics governance slice can define aggregation and deletion retention without changing the public interaction.

## Current boundaries

- Private Administrator authentication protects Admin mutations; no public visitor authentication or account system exists.
- Media can be selected from existing metadata, but upload and storage-provider administration remain Issue #49.
- Existing Series associations are readable; complete Series Admin, ordering, and Reading Paths remain Issue #39.
- Missing optional images/PDFs use deliberate fallback states and never render broken actions.
