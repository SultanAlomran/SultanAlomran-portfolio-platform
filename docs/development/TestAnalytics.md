# Test Analytics development guide

Test Analytics is the permanent quality-observability layer for Portfolio Platform. SQL Server stores normalized run, test-case, and artifact metadata; it never stores screenshots, videos, traces, report ZIPs, or HTML report binaries.

## Data and artifact model

- `TestRun` records provider/run identity, Git context, execution mode, status, timing, and aggregate counts.
- `TestCaseResult` records feature, suite, browser, viewport, duration, retries, flaky state, and a bounded error summary.
- `TestArtifact` records type, provider identifier, external URL/storage path, size, retention date, and availability (`Available`, `Expired`, `Deleted`, or `Archived`).
- `(Provider, ProviderRunId)` is unique, making imports idempotent.

GitHub Actions Artifacts is the initial evidence provider. Artifact URLs are not permanent; telemetry remains useful after evidence expires. `AzureBlob` is an existing provider value so selected evidence can be archived later without changing the relational model. Azure Blob integration is intentionally not implemented yet.

## Artifact production and storage

Playwright writes per-test screenshots, `video.webm`, and `trace.zip` below repository-local `test-results/`. Normalized JSON/JUnit/telemetry are written to `test-results/results.json`, `test-results/junit.xml`, and `test-results/telemetry.json`; the HTML report is written to `playwright-report/`. Full-recording and visual journeys also write named evidence PNGs below each test's `evidence/` directory.

GitHub workflows upload those directories as ZIP-backed Actions artifacts. GitHub's artifact download API returns a redirect to a short-lived ZIP download; it is not an individual image/video URL and cannot be used safely as an `<img>` or `<video>` source. SQL stores only the file metadata and repository-relative path.

Explicit workflow retention is currently:

- telemetry: 90 days;
- HTML reports and failure diagnostics: 14 days;
- visual evidence: 21 days;
- full recordings: 30 days.

After retention expires, metadata remains in SQL but preview/download returns a friendly unavailable/expired state.

## Ingestion stages

Playwright writes JSON and JUnit reports. `tests/playwright/scripts/build-telemetry.mjs` normalizes the JSON report, GitHub run context, and artifact metadata into ignored `test-results/telemetry.json`. CI uploads this file as a 90-day GitHub artifact but cannot write to a developer-local SQL Server.

For local development, start the API in Development, download or generate `telemetry.json`, then run:

```powershell
.\scripts\import-test-telemetry.ps1
```

The import API is deliberately mapped only in Development while authentication is deferred. A future hosted API can expose a protected ingestion endpoint for GitHub Actions; credentials must remain server-side and must never be sent to Portfolio.Admin.

## Routes

Admin UI: `/quality/tests` and `/quality/tests/runs/:id`.

Read APIs under `/api/admin/test-analytics`:

- `GET /overview`, `/runs`, `/runs/{id}`, `/tests`, `/flaky`, `/browsers`, `/features`, `/trends`
- `GET /runs/{id}/artifacts`
- `GET /artifacts/{artifactId}/content` for restricted screenshot/video preview
- `GET /artifacts/{artifactId}/content?download=true` for a known file or report archive download
- `POST /import` (Development only)

Filters support date range, branch, status, browser, feature, execution mode, paging, and stable sorting where applicable.

## Preview resolver

The content endpoint accepts only a `TestArtifact.Id` already stored in Test Analytics. It never accepts an external URL or filesystem path. The resolver validates the stored relative path, blocks rooted and traversal paths, restricts inline MIME types to PNG/JPEG/WebP and WebM/MP4, enforces archive/file size and request timeout limits, and streams a selected file from disk. ASP.NET Core range processing is enabled for video, so native browser seeking produces byte-range responses without loading the entire recording into memory.

Resolution order is:

1. repository-relative local evidence (`TestArtifacts:LocalRoot`) for local development;
2. a server-side GitHub Actions provider for `GitHubActions` metadata;
3. a clear unavailable response when the file/provider/credential has expired or is missing.

GitHub ZIPs strip the common `test-results/` prefix. The resolver understands both forms and the specific visual convention that maps hashed `attachments/<name>-<sha>.png` metadata to the same test's `evidence/<name>.png`. It extracts only the approved selected entry to a bounded server cache. HTML report downloads return the known report container as a ZIP.

Configure the GitHub provider through server configuration/user secrets, never Angular:

- `TestArtifacts:GitHub:Repository`
- `TestArtifacts:GitHub:Token` (Actions read access only)

No token is committed. The browser sees only the Portfolio.Api artifact-ID URL. Azure Preview will return “GitHub artifact preview is not configured on this server” until a server-side secret and the platform's deferred Admin authentication are configured.

## Known limits

- Existing GitHub history is not fabricated or automatically backfilled. Import begins with real telemetry files that are available.
- GitHub artifacts are ZIP containers, so the first remote preview downloads and extracts the selected entry into the bounded server cache; subsequent video range requests use that cached file.
- Platform authentication and Azure Blob archival remain future slices. Do not enable a GitHub token on a publicly reachable Admin/API deployment until the existing Admin authentication slice is implemented.
