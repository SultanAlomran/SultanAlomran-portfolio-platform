[CmdletBinding()]
param([switch]$DeleteTunnel)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$stateRoot = Join-Path $repoRoot '.dev-tunnel'
$idFile = Join-Path $stateRoot 'tunnel-id.txt'
$hostStateFile = Join-Path $stateRoot 'host.json'

if (Test-Path $hostStateFile) {
    $hostState = Get-Content -Raw $hostStateFile | ConvertFrom-Json
    if (Get-Process -Id $hostState.ProcessId -ErrorAction SilentlyContinue) {
        $termination = Start-Process -FilePath 'taskkill.exe' `
            -ArgumentList @('/PID', [string]$hostState.ProcessId, '/T', '/F') `
            -NoNewWindow -Wait -PassThru
        if ($termination.ExitCode -ne 0 -and (Get-Process -Id $hostState.ProcessId -ErrorAction SilentlyContinue)) {
            throw "Could not stop Dev Tunnel host process tree (PID $($hostState.ProcessId))."
        }
        Write-Host "STOPPED  Dev Tunnel host PID $($hostState.ProcessId)"
    }
    Remove-Item -LiteralPath $hostStateFile -Force
}
else {
    Write-Host 'No running Dev Tunnel host state was found.'
}

if ($DeleteTunnel -and (Test-Path $idFile)) {
    $tunnelId = (Get-Content -Raw $idFile).Trim()
    $devTunnel = (Get-Command devtunnel -ErrorAction SilentlyContinue).Source
    if (-not $devTunnel) {
        $packageRoot = Join-Path $env:LOCALAPPDATA 'Microsoft/WinGet/Packages'
        $devTunnel = (Get-ChildItem $packageRoot -Filter devtunnel.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1).FullName
    }
    if (-not $devTunnel) { throw 'Microsoft Dev Tunnel CLI was not found.' }
    & $devTunnel delete $tunnelId --force
    if ($LASTEXITCODE -ne 0) { throw "Could not delete Dev Tunnel $tunnelId." }
    Remove-Item -LiteralPath $idFile -Force
    Write-Host "DELETED  Dev Tunnel $tunnelId"
}
