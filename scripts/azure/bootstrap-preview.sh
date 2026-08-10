#!/usr/bin/env bash
set -euo pipefail
: "${AZURE_LOCATION:?Set AZURE_LOCATION after choosing a region.}"
: "${GITHUB_REPOSITORY:=SultanAlomran/SultanAlomran-portfolio-platform}"
subscription_id="$(az account show --query id -o tsv)"
suffix="$(printf '%s' "$subscription_id" | tr -d '-' | tail -c 7)"
resource_group="${AZURE_PREVIEW_RESOURCE_GROUP:-portfolio-preview}"
app_name="portfolio-preview-github"
az group create --name "$resource_group" --location "$AZURE_LOCATION" --tags repository="$GITHUB_REPOSITORY" environment=preview-shared
app_id="$(az ad app create --display-name "$app_name" --query appId -o tsv)"
object_id="$(az ad sp create --id "$app_id" --query id -o tsv)"
scope="/subscriptions/$subscription_id/resourceGroups/$resource_group"
az role assignment create --assignee-object-id "$object_id" --assignee-principal-type ServicePrincipal --role Contributor --scope "$scope"
# Required only to grant each Container App identity AcrPull on the shared registry.
az role assignment create --assignee-object-id "$object_id" --assignee-principal-type ServicePrincipal --role "Role Based Access Control Administrator" --scope "$scope"
cat > /tmp/portfolio-preview-federation.json <<JSON
{"name":"portfolio-preview-dev-prs","issuer":"https://token.actions.githubusercontent.com","subject":"repo:${GITHUB_REPOSITORY}:environment:azure-preview","description":"PR preview deployments","audiences":["api://AzureADTokenExchange"]}
JSON
az ad app federated-credential create --id "$app_id" --parameters /tmp/portfolio-preview-federation.json
rm /tmp/portfolio-preview-federation.json
sql_password="$(openssl rand -base64 36 | tr -d '/+=' | head -c 32)Aa1!"
az deployment group create --resource-group "$resource_group" --template-file infra/azure/preview/shared.bicep --parameters suffix="$suffix" location="$AZURE_LOCATION" sqlAdministratorPassword="$sql_password"
printf 'Configure GitHub environment azure-preview with variables:\nAZURE_CLIENT_ID=%s\nAZURE_TENANT_ID=%s\nAZURE_SUBSCRIPTION_ID=%s\nAZURE_LOCATION=%s\nAZURE_PREVIEW_RESOURCE_GROUP=%s\nAZURE_RESOURCE_SUFFIX=%s\n' "$app_id" "$(az account show --query tenantId -o tsv)" "$subscription_id" "$AZURE_LOCATION" "$resource_group" "$suffix"
printf 'Store this generated value as environment secret AZURE_SQL_ADMIN_PASSWORD (shown once): %s\n' "$sql_password"
