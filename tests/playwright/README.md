# Playwright E2E foundation

This directory owns end-to-end testing for Portfolio.Admin and Portfolio.Web. Tests run against local applications and a local/disposable SQL Server database; Microsoft Dev Tunnel is not required.

## Execution modes

- **Standard** is the normal pull-request check. Chromium runs the high-value smoke suite; screenshots, video, and traces are retained only on failure, and an HTML/JUnit report is produced.
- **Visual evidence** runs tests tagged `@visual` when the PR has the `e2e-visual` label or the manual workflow selects `visual`. Selected successful screenshots are uploaded as artifacts. Stable visual assertions use `toHaveScreenshot`; CI never updates baselines.
- **Full recording** runs tests tagged `@record` when the PR has the `e2e-record` label or the manual workflow selects `full-recording`. Successful screenshots, video, trace, and the HTML report are retained for review.

The visual and recording modes are supplemental and are not mandatory checks for ordinary PRs.

## Install and run locally

Use Node `>=22.12.0 <23`, restore all three lockfiles, and install the required browser once:

```powershell
npm ci
npm --prefix src/Portfolio.Admin ci
npm --prefix src/Portfolio.Web ci
npx playwright install chromium
npm run e2e:smoke
```

Playwright starts API (`5100`), Web (`4200`), and Admin (`4300`), waits for readiness, and stops the processes it owns. Existing servers are reused locally. Supply `ConnectionStrings__PortfolioDatabase` or the normal API user-secret, and apply migrations before tests that need Projects data. CI creates and migrates an isolated SQL Server database.

Useful commands:

```powershell
npm run e2e
npm run e2e:admin
npm run e2e:web
npm run e2e:responsive
npm run e2e:quality
npm run e2e:visual
npm run e2e:record
npm run e2e:headed
npm run e2e:debug
npm run e2e:report
node tests/playwright/scripts/run.mjs --suite projects --browser firefox
node tests/playwright/scripts/build-telemetry.mjs --mode standard --feature quality --browser chromium
```

Set `E2E_MANAGE_SERVERS=false` when all applications are already running. Override `API_BASE_URL`, `WEB_BASE_URL`, or `ADMIN_BASE_URL` when necessary. `E2E_BROWSER` supports Chromium, Firefox, and WebKit; the default PR path uses Chromium to control duration.

## Architecture and conventions

Feature tests belong under `admin/<feature>/` or `public/<feature>/`. Cross-application smoke tests stay at the application root, and targeted viewport checks live under `responsive/`. Reusable fixtures, helpers, page objects, components, and deterministic test data have dedicated folders. Add abstractions only when two or more tests genuinely benefit.

Prefer `getByRole`, `getByLabel`, `getByText`, and `getByTestId` over DOM/CSS structure. Page objects model reusable user intent, not every element. The diagnostics fixture fails tests on console errors, uncaught exceptions, failed requests, unexpected 404 assets, and 500+ responses. Explicitly allowlist an expected response, such as an intentional unknown-project 404, inside that test. Keyboard focus advancement is asserted in Chromium/Firefox; the Windows WebKit test port clears focus for synthetic Tab events, so WebKit remains focused on rendering, semantics, routing, and runtime diagnostics.

Future vertical slices should add:

1. a fast smoke/critical-journey scenario;
2. valuable validation or error coverage;
3. targeted responsive checks rather than multiplying every test across every viewport;
4. an `@visual` evidence scenario where design review benefits;
5. an `@record` journey only for acceptance or demonstration.

Tests must use an isolated database before creating or deleting data. Authentication is deferred; do not add a production bypass. The current wizard coverage performs client-side validation and navigation without saving developer records. Add API-assisted setup/cleanup when authenticated, isolated E2E data support is approved.

## Visual snapshots and artifacts

Create or review a baseline locally with:

```powershell
node tests/playwright/scripts/run.mjs --mode visual --update-snapshots
npm run e2e:visual
```

Commit a baseline only after intentional review. Baselines are platform-specific so Windows local review and Linux CI compare like-for-like. The initial reviewed baseline is Windows-only; Linux CI still captures evidence but does not perform a pixel comparison until a Linux baseline is generated with the official Playwright container, reviewed, committed, and `E2E_SNAPSHOT_PLATFORMS` includes `linux`. Never auto-accept snapshots in CI. Runtime output is ignored under `test-results/`, `playwright-report/`, `blob-report/`, and `.playwright-artifacts/`. Open traces with `npx playwright show-trace <trace.zip>` and the report with `npm run e2e:report`.

## GitHub Actions

Every applicable PR runs the standard smoke suite after .NET and Angular validation. Apply `e2e-visual` for successful visual evidence or `e2e-record` for full workflow evidence. The **Playwright E2E** workflow can also be dispatched manually with feature (`smoke`, `projects`, `quality`, `all`), evidence (`standard`, `visual`, `full-recording`), and browser inputs. Reports are retained for 14 days, visual evidence for 21 days, and full recordings for 30 days.

Each CI run also generates ignored `test-results/telemetry.json` from the Playwright JSON report and uploads it for 90 days. The normalized file contains run, test, and artifact metadata only—never evidence binaries or credentials—and can be imported into Test Analytics. CI does not try to connect to a developer-local SQL Server.

If startup fails, verify ports are free, the connection string points only to a local/disposable database, migrations exist, and all lockfiles were restored. Inspect the HTML report and attached `browser-diagnostics` before weakening an assertion or allowlist.
