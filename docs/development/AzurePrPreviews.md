# Azure pull-request previews

## Purpose and decision

Issue #32 is interpreted as an internet-accessible, temporary review environment for PRs targeting `dev`, independent of a developer PC. It complements Microsoft Dev Tunnel (local/uncommitted work) and is not the future production deployment described by issue #41.

The selected design uses a **shared Azure Container Apps consumption environment**, **shared Basic Azure Container Registry**, and **shared Azure SQL logical server**. Each labeled PR gets three scale-to-zero Container Apps (Web, Admin, API) and its own Basic SQL database. A shared resource group is cheaper and avoids an environment/registry per PR; deterministic names and tags still make targeted cleanup safe.

Alternatives considered:

* Static Web Apps plus Container Apps reduces frontend runtime, but coordinating two native PR systems, API CORS/URLs, and teardown is more complex.
* A Container Apps environment and SQL Server container per PR makes deletion simple but SQL Server's memory and persistent storage make it costlier and less reliable.
* App Service has straightforward .NET hosting but always-on plans and per-PR slots are a worse isolation/cost fit.

## One-time owner bootstrap

1. Create an Azure account/subscription if none exists. Install Azure CLI, then authenticate without sharing credentials: `az login` (or `az login --use-device-code`). Select the correct subscription with `az account set --subscription <id>`.
2. Verify service availability/pricing and select a region, for example `export AZURE_LOCATION=uaenorth`. Run `scripts/azure/bootstrap-preview.sh` from the repository root.
3. Create a protected GitHub environment named `azure-preview`. Add the printed `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_LOCATION`, `AZURE_PREVIEW_RESOURCE_GROUP`, and `AZURE_RESOURCE_SUFFIX` as environment variables. Add the generated password as environment secret `AZURE_SQL_ADMIN_PASSWORD`.
4. Add the `azure-preview` repository label. Optionally require an environment reviewer, especially for fork PRs.

The bootstrap creates an Entra application, a GitHub OIDC federated credential, and Contributor and Role Based Access Control Administrator assignments limited to the preview resource group. The latter is required only so deployments can grant each Container App identity `AcrPull` on the shared registry. GitHub receives short-lived tokens through `azure/login`; no service-principal password exists. Contributor is needed to provision apps/databases and delete them; the RBAC role cannot manage ordinary Azure resources. The generated SQL administrator secret is separate from Azure authentication and must be rotated/kept only in the GitHub environment. For stricter operations, replace Contributor with a custom role containing only the resource types in the templates.

## Lifecycle and URLs

Add the `azure-preview` label to an open PR targeting `dev`. Open/reopen, label, and subsequent synchronize events validate builds, build images, deploy/migrate/seed, run four small HTTP smoke checks, and update one marker comment with HTTPS Web, Admin, API, health, Scalar, and OpenAPI URLs. The GitHub environment also displays the Web URL. Different PR numbers have separate concurrency groups, apps, and databases; a newer run supersedes an older run for the same PR.

Closing/merging runs idempotent targeted cleanup. Manual cleanup is available from the cleanup workflow with a validated PR number. A weekly workflow enumerates tagged preview apps and deletes only those whose GitHub PR reports `closed`; API errors/unknown PR state are retained safely. Shared infrastructure and old ACR images require occasional owner review and are never removed by a PR cleanup.

## Data, API, and security

Migrations and the deterministic development project seed run against only `portfolio-pr-<number>`. No production/local database or copied user data is used. The temporary runner firewall rule is removed with a shell trap. API configuration is injected using environment variables; only that PR's exact Web/Admin origins are allowed by CORS.

`Preview` is an explicit ASP.NET Core environment. Scalar and `/openapi/v1.json` are enabled there for review and remain disabled in Production. Admin authentication is not invented: its preview is public and clearly labeled until product authentication exists. Frontends send `X-Robots-Tag: noindex, nofollow, noarchive`; this discourages indexing but is **not authentication**. Never place production secrets, personal data, private messages, or confidential files in a preview.

## Operations and troubleshooting

* Dev Tunnel remains the tool for local/uncommitted testing; these hosted URLs work while the developer PC is off and have no tunnel dependency.
* If login fails, compare the GitHub environment name and OIDC federated subject, then verify all six environment variables.
* If readiness fails, inspect Container App system logs live; permanent Log Analytics is intentionally disabled to avoid recurring per-PR logging resources.
* If migration cannot connect, confirm the runner firewall rule was created and SQL public access was not disabled.
* Scale-from-zero and first deployment can produce a cold-start delay. Azure SQL Basic bills continuously until cleanup; ACR persists and can also cost money.
* Production resources/configuration are not referenced or changed by these templates.
