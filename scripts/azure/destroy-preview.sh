#!/usr/bin/env bash
set -euo pipefail
[[ "${1:-}" =~ ^[1-9][0-9]{0,5}$ ]] || { echo 'PR number must be 1-999999.' >&2; exit 2; }
: "${AZURE_PREVIEW_RESOURCE_GROUP:?required}"; : "${AZURE_SQL_SERVER_NAME:?required}"
pr="$1"
for app in api web admin; do az containerapp delete --yes --name "portfolio-${app}-pr-${pr}" --resource-group "$AZURE_PREVIEW_RESOURCE_GROUP" 2>/dev/null || true; done
az sql db delete --yes --server "$AZURE_SQL_SERVER_NAME" --name "portfolio-pr-${pr}" --resource-group "$AZURE_PREVIEW_RESOURCE_GROUP" 2>/dev/null || true
