#!/usr/bin/env bash
set -euo pipefail
[[ "${1:-}" =~ ^[1-9][0-9]{0,5}$ ]] || { echo 'PR number must be 1-999999.' >&2; exit 2; }
: "${AZURE_PREVIEW_RESOURCE_GROUP:?required}"; : "${AZURE_SQL_SERVER_NAME:?required}"; : "${AZURE_CONTAINER_REGISTRY_NAME:?required}"
pr="$1"
registry_id="$(az acr show --name "$AZURE_CONTAINER_REGISTRY_NAME" --resource-group "$AZURE_PREVIEW_RESOURCE_GROUP" --query id --output tsv)"
for app in api web admin; do
  app_name="portfolio-${app}-pr-${pr}"
  principal_id="$(az containerapp show --name "$app_name" --resource-group "$AZURE_PREVIEW_RESOURCE_GROUP" --query identity.principalId --output tsv 2>/dev/null || true)"
  if [[ -n "$principal_id" ]]; then
    az role assignment delete --assignee-object-id "$principal_id" --role AcrPull --scope "$registry_id" 2>/dev/null || true
  fi
  az containerapp delete --yes --name "$app_name" --resource-group "$AZURE_PREVIEW_RESOURCE_GROUP" 2>/dev/null || true
done
az sql db delete --yes --server "$AZURE_SQL_SERVER_NAME" --name "portfolio-pr-${pr}" --resource-group "$AZURE_PREVIEW_RESOURCE_GROUP" 2>/dev/null || true
