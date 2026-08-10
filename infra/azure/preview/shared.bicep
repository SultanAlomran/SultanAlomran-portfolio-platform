targetScope = 'resourceGroup'
@description('Short globally unique suffix, for example the final six characters of the subscription ID.')
@minLength(3)
@maxLength(10)
param suffix string
param location string
@secure()
param sqlAdministratorPassword string
param sqlAdministratorLogin string = 'portfolioadmin'

var baseName = 'portfolio-preview-${toLower(suffix)}'
resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: replace('${baseName}acr', '-', '')
  location: location
  sku: { name: 'Basic' }
  properties: { adminUserEnabled: false }
  tags: { repository: 'SultanAlomran-portfolio-platform', environment: 'preview-shared' }
}
resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${baseName}-env'
  location: location
  properties: { appLogsConfiguration: { destination: 'none' } }
  tags: { repository: 'SultanAlomran-portfolio-platform', environment: 'preview-shared' }
}
resource sql 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: '${baseName}-sql'
  location: location
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
  tags: { repository: 'SultanAlomran-portfolio-platform', environment: 'preview-shared' }
}
output registryName string = registry.name
output registryServer string = registry.properties.loginServer
output environmentId string = environment.id
output sqlServerName string = sql.name
output sqlServerFqdn string = sql.properties.fullyQualifiedDomainName
