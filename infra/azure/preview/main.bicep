targetScope = 'resourceGroup'
param location string
@minValue(1)
@maxValue(999999)
param prNumber int
param containerAppsEnvironmentId string
param registryName string
param sqlServerName string
param sqlAdministratorLogin string
@secure()
param sqlAdministratorPassword string
param imageTag string
@description('Creates or repairs the Container App identities with a public scale-to-zero image before private ACR images are applied.')
param identityBootstrap bool = false
param repository string = 'SultanAlomran/SultanAlomran-portfolio-platform'

var suffix = 'pr-${prNumber}'
var registryServer = '${registryName}.azurecr.io'
var apiName = 'portfolio-api-${suffix}'
var webName = 'portfolio-web-${suffix}'
var adminName = 'portfolio-admin-${suffix}'
var dbName = 'portfolio-pr-${prNumber}'
var commonTags = { repository: repository, environment: 'preview', pr: string(prNumber) }
var bootstrapImage = 'mcr.microsoft.com/k8se/quickstart:latest'
var registryConfiguration = identityBootstrap ? [] : [{ server: registryServer, identity: 'system' }]
resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = { name: registryName }
resource environment 'Microsoft.App/managedEnvironments@2024-03-01' existing = { name: last(split(containerAppsEnvironmentId, '/')) }
resource sql 'Microsoft.Sql/servers@2023-08-01-preview' existing = { name: sqlServerName }
resource database 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sql
  name: dbName
  location: location
  sku: { name: 'Basic', tier: 'Basic', capacity: 5 }
  properties: { maxSizeBytes: 2147483648 }
  tags: commonTags
}
resource api 'Microsoft.App/containerApps@2024-03-01' = {
  name: apiName
  location: location
  identity: { type: 'SystemAssigned' }
  tags: commonTags
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: { external: !identityBootstrap, targetPort: identityBootstrap ? 80 : 8080, transport: 'auto', allowInsecure: false }
      registries: registryConfiguration
      secrets: identityBootstrap ? [] : [{ name: 'database', value: 'Server=tcp:${sql.properties.fullyQualifiedDomainName},1433;Initial Catalog=${dbName};User ID=${sqlAdministratorLogin};Password=${sqlAdministratorPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;' }]
    }
    template: {
      containers: [{
        name: 'api'
        image: identityBootstrap ? bootstrapImage : '${registryServer}/portfolio-api:${imageTag}'
        env: identityBootstrap ? [] : [
          { name: 'ASPNETCORE_ENVIRONMENT', value: 'Preview' }
          { name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED', value: 'true' }
          { name: 'ConnectionStrings__PortfolioDatabase', secretRef: 'database' }
          { name: 'Cors__AllowedOrigins__0', value: 'https://${webName}.${environment.properties.defaultDomain}' }
          { name: 'Cors__AllowedOrigins__1', value: 'https://${adminName}.${environment.properties.defaultDomain}' }
        ]
        resources: { cpu: json('0.25'), memory: '0.5Gi' }
      }]
      scale: { minReplicas: 0, maxReplicas: 1 }
    }
  }
}
resource apiPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, api.id, 'AcrPull')
  scope: registry
  properties: { principalId: api.identity.principalId, principalType: 'ServicePrincipal', roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') }
}
resource web 'Microsoft.App/containerApps@2024-03-01' = {
  name: webName
  location: location
  identity: { type: 'SystemAssigned' }
  tags: commonTags
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: { activeRevisionsMode: 'Single', ingress: { external: !identityBootstrap, targetPort: identityBootstrap ? 80 : 8080, allowInsecure: false }, registries: registryConfiguration }
    template: { containers: [{ name: 'web', image: identityBootstrap ? bootstrapImage : '${registryServer}/portfolio-web:${imageTag}', env: identityBootstrap ? [] : [{ name: 'PREVIEW_API_URL', value: 'https://${api.properties.configuration.ingress.fqdn}' }], resources: { cpu: json('0.25'), memory: '0.5Gi' } }], scale: { minReplicas: 0, maxReplicas: 1 } }
  }
}
resource webPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = { name: guid(registry.id, web.id, 'AcrPull'), scope: registry, properties: { principalId: web.identity.principalId, principalType: 'ServicePrincipal', roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') } }
resource admin 'Microsoft.App/containerApps@2024-03-01' = {
  name: adminName
  location: location
  identity: { type: 'SystemAssigned' }
  tags: commonTags
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: { activeRevisionsMode: 'Single', ingress: { external: !identityBootstrap, targetPort: identityBootstrap ? 80 : 8080, allowInsecure: false }, registries: registryConfiguration }
    template: { containers: [{ name: 'admin', image: identityBootstrap ? bootstrapImage : '${registryServer}/portfolio-admin:${imageTag}', env: identityBootstrap ? [] : [{ name: 'PREVIEW_API_URL', value: 'https://${api.properties.configuration.ingress.fqdn}' }], resources: { cpu: json('0.25'), memory: '0.5Gi' } }], scale: { minReplicas: 0, maxReplicas: 1 } }
  }
}
resource adminPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = { name: guid(registry.id, admin.id, 'AcrPull'), scope: registry, properties: { principalId: admin.identity.principalId, principalType: 'ServicePrincipal', roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') } }
output apiUrl string = 'https://${api.properties.configuration.ingress.fqdn}'
output webUrl string = 'https://${web.properties.configuration.ingress.fqdn}'
output adminUrl string = 'https://${admin.properties.configuration.ingress.fqdn}'
output databaseName string = database.name
