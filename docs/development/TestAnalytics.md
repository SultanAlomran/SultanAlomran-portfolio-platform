# Test Analytics development guide

Test Analytics is the permanent quality-observability layer for Portfolio Platform. SQL Server stores normalized run, test-case, and artifact metadata; it never stores screenshots, videos, traces, report ZIPs, or HTML report binaries.

## Data and artifact model

- `TestRun` records provider/run identity, Git context, execution mode, status, timing, and aggregate counts.
- `TestCaseResult` records feature, suite, browser, viewport, duration, retries, flaky state, and a bounded error summary.
- `TestArtifact` records type, provider identifier, external URL/storage path, size, retention date, and availability (`Available`, `Expired`, `Deleted`, or `Archived`).
- `(Provider, ProviderRunId)` is unique, making imports idempotent.

GitHub Actions Artifacts is the initial evidence provider. Artifact URLs are not permanent; telemetry remains useful after evidence expires. `AzureBlob` is an existing provider value so selected evidence can be archived later without changing the relational model. Azure Blob integration is intentionally not implemented yet.

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
- `POST /import` (Development only)

Filters support date range, branch, status, browser, feature, execution mode, paging, and stable sorting where applicable. GitHub links contain no tokens; artifact opening still follows GitHub's authenticated access rules.

## Known limits

- Existing GitHub history is not fabricated or automatically backfilled. Import begins with real telemetry files that are available.
- GitHub artifact IDs/direct download URLs are not known until upload completes. Initial records link to the authenticated workflow run; a future server-side GitHub synchronizer can resolve provider artifact IDs and fresh download URLs.
- Authentication and Azure Blob archival remain future slices.
