[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'start-local.ps1')
& (Join-Path $PSScriptRoot 'dev-tunnel-start.ps1')
