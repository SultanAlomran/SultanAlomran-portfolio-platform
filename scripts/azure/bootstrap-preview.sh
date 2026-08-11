#!/usr/bin/env bash
set -euo pipefail
export MSYS_NO_PATHCONV=1
: "${AZURE_LOCATION:?Set AZURE_LOCATION after choosing a region.}"
: "${GITHUB_REPOSITORY:=SultanAlomran/SultanAlomran-portfolio-platform}"
command -v gh >/dev/null 2>&1 || { echo "GitHub CLI is required to configure the azure-preview environment securely." >&2; exit 1; }
gh auth status >/dev/null 2>&1 || { echo "Authenticate GitHub CLI before running bootstrap." >&2; exit 1; }
subscription_id="$(az account show --query id -o tsv | tr -d '\r')"
suffix="$(printf '%s' "$subscription_id" | tr -d '-' | tail -c 7)"
resource_group="${AZURE_PREVIEW_RESOURCE_GROUP:-portfolio-preview}"
app_name="portfolio-preview-github"
for provider in Microsoft.App Microsoft.ContainerRegistry Microsoft.Sql; do
  az provider register --namespace "$provider" --wait
done
az group create --name "$resource_group" --location "$AZURE_LOCATION" --tags repository="$GITHUB_REPOSITORY" environment=preview-shared
mapfile -t app_ids < <(az ad app list --display-name "$app_name" --query '[].appId' -o tsv | tr -d '\r')
if (( ${#app_ids[@]} > 1 )); then
  echo "Multiple Entra applications named $app_name exist; refusing to choose one." >&2
  exit 1
elif (( ${#app_ids[@]} == 1 )); then
  app_id="${app_ids[0]}"
else
  app_id="$(az ad app create --display-name "$app_name" --query appId -o tsv | tr -d '\r')"
fi
object_id="$(az ad sp show --id "$app_id" --query id -o tsv 2>/dev/null | tr -d '\r' || true)"
if [[ -z "$object_id" ]]; then
  object_id="$(az ad sp create --id "$app_id" --query id -o tsv | tr -d '\r')"
fi
scope="/subscriptions/$subscription_id/resourceGroups/$resource_group"
az role assignment create --assignee-object-id "$object_id" --assignee-principal-type ServicePrincipal --role Contributor --scope "$scope"
# Required only to grant each Container App identity AcrPull on the shared registry.
az role assignment create --assignee-object-id "$object_id" --assignee-principal-type ServicePrincipal --role "Role Based Access Control Administrator" --scope "$scope"
federation_json="{\"name\":\"portfolio-preview-dev-prs\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:${GITHUB_REPOSITORY}:environment:azure-preview\",\"description\":\"PR preview deployments\",\"audiences\":[\"api://AzureADTokenExchange\"]}"
if [[ "$(az ad app federated-credential list --id "$app_id" --query "[?name=='portfolio-preview-dev-prs'] | length(@)" -o tsv | tr -d '\r')" == 0 ]]; then
  az ad app federated-credential create --id "$app_id" --parameters "$federation_json"
fi
sql_password="$(openssl rand -base64 36 | tr -d '/+=' | head -c 32)Aa1!"
az deployment group create --resource-group "$resource_group" --template-file infra/azure/preview/shared.bicep --parameters suffix="$suffix" location="$AZURE_LOCATION" sqlAdministratorPassword="$sql_password"
tenant_id="$(az account show --query tenantId -o tsv | tr -d '\r')"
gh api --method PUT "repos/${GITHUB_REPOSITORY}/environments/azure-preview" >/dev/null
printf '%s' "$sql_password" | gh secret set AZURE_SQL_ADMIN_PASSWORD --env azure-preview --repo "$GITHUB_REPOSITORY"
gh variable set AZURE_CLIENT_ID --env azure-preview --repo "$GITHUB_REPOSITORY" --body "$app_id"
gh variable set AZURE_TENANT_ID --env azure-preview --repo "$GITHUB_REPOSITORY" --body "$tenant_id"
gh variable set AZURE_SUBSCRIPTION_ID --env azure-preview --repo "$GITHUB_REPOSITORY" --body "$subscription_id"
gh variable set AZURE_LOCATION --env azure-preview --repo "$GITHUB_REPOSITORY" --body "$AZURE_LOCATION"
gh variable set AZURE_PREVIEW_RESOURCE_GROUP --env azure-preview --repo "$GITHUB_REPOSITORY" --body "$resource_group"
gh variable set AZURE_RESOURCE_SUFFIX --env azure-preview --repo "$GITHUB_REPOSITORY" --body "$suffix"
unset sql_password
printf 'Configured GitHub environment azure-preview with variables:\nAZURE_CLIENT_ID\nAZURE_TENANT_ID\nAZURE_SUBSCRIPTION_ID\nAZURE_LOCATION\nAZURE_PREVIEW_RESOURCE_GROUP\nAZURE_RESOURCE_SUFFIX\nand secret:\nAZURE_SQL_ADMIN_PASSWORD\n'
