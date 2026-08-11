# Azure PR preview infrastructure

`shared.bicep` creates the deliberately small shared foundation once. `main.bicep` idempotently creates three consumption Container Apps and one isolated Basic Azure SQL database per opted-in PR. Shared resources are not deleted by PR cleanup; per-PR resources are tagged and deleted by name.

Choose and export `AZURE_LOCATION` before running `scripts/azure/bootstrap-preview.sh`. Do not choose a region blindly: confirm Container Apps and Azure SQL availability, latency, data residency, and prices for the subscription. The suggested starting point for an owner in Saudi Arabia is `uaenorth`, subject to that check.

The templates intentionally disable permanent Container Apps log storage. Container Apps have zero minimum replicas. Azure SQL Basic does not scale to zero and accrues cost until deletion. ACR Basic and the shared SQL logical server/Container Apps environment persist; the registry has a monthly charge, while the logical server itself generally is not the database billing unit. Image storage/build, executions, bandwidth, and databases can cost money. Azure pricing and subscription benefits must be checked; this design is not promised to be free.
