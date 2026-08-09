[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
& (Join-Path $PSScriptRoot 'dev-tunnel-stop.ps1')
& (Join-Path $PSScriptRoot 'stop-local.ps1')
