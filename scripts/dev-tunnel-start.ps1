[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$stateRoot = Join-Path $repoRoot '.dev-tunnel'
$idFile = Join-Path $stateRoot 'tunnel-id.txt'
$hostStateFile = Join-Path $stateRoot 'host.json'
$hostOutLog = Join-Path $stateRoot 'host.out.log'
$hostErrLog = Join-Path $stateRoot 'host.err.log'

function Find-DevTunnel {
    $command = Get-Command devtunnel -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }

    $packageRoot = Join-Path $env:LOCALAPPDATA 'Microsoft/WinGet/Packages'
    $candidate = Get-ChildItem $packageRoot -Filter devtunnel.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($candidate) { return $candidate.FullName }

    throw 'Microsoft Dev Tunnel CLI is not installed. Run: winget install --id Microsoft.devtunnel --exact'
}

function Invoke-DevTunnel([string[]]$Arguments, [switch]$AllowFailure) {
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & $script:devTunnel @Arguments 2>&1 | Out-String
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }
    if (-not $AllowFailure -and $exitCode -ne 0) { throw $output.Trim() }
    return [pscustomobject]@{ ExitCode = $exitCode; Output = $output.Trim() }
}

$devTunnel = Find-DevTunnel
$login = Invoke-DevTunnel @('user', 'show') -AllowFailure
if ($login.ExitCode -ne 0 -or $login.Output -match 'Not logged in') {
    throw @'
Microsoft Dev Tunnel authentication is required.
Run this command in a terminal and complete the browser sign-in:
  devtunnel user login
For device-code sign-in:
  devtunnel user login -d
Then rerun scripts/dev-tunnel-start.ps1.
'@
}

foreach ($port in 5100, 4200, 4300) {
    if (-not (Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue)) {
        throw "Local port $port is not listening. Run scripts/start-local.ps1 first."
    }
}

New-Item -ItemType Directory -Force -Path $stateRoot | Out-Null

if (Test-Path $hostStateFile) {
    $hostState = Get-Content -Raw $hostStateFile | ConvertFrom-Json
    if (Get-Process -Id $hostState.ProcessId -ErrorAction SilentlyContinue) {
        Write-Host "Dev Tunnel is already running with PID $($hostState.ProcessId)."
        if (Test-Path $hostOutLog) { Get-Content $hostOutLog | Where-Object { $_ -match 'https://' } }
        exit 0
    }
}

$tunnelId = if (Test-Path $idFile) { (Get-Content -Raw $idFile).Trim() } else { '' }
if ($tunnelId) {
    $existing = Invoke-DevTunnel @('show', $tunnelId) -AllowFailure
    if ($existing.ExitCode -ne 0) { $tunnelId = '' }
}

if (-not $tunnelId) {
    $suffix = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $tunnelId = "portfolio-$suffix"
    Invoke-DevTunnel @('create', $tunnelId, '--description', 'Portfolio Platform local development') | Out-Null
    Set-Content -Path $idFile -Value $tunnelId -Encoding ascii
    Write-Host "CREATED private tunnel $tunnelId"
}
else {
    Write-Host "REUSING private tunnel $tunnelId"
}

$ports = @(
    @{ Number = 5100; Description = 'Portfolio.Api' },
    @{ Number = 4200; Description = 'Portfolio.Web' },
    @{ Number = 4300; Description = 'Portfolio.Admin' }
)

foreach ($port in $ports) {
    $existingPort = Invoke-DevTunnel @('port', 'show', $tunnelId, '--port-number', [string]$port.Number) -AllowFailure
    if ($existingPort.ExitCode -ne 0) {
        Invoke-DevTunnel @('port', 'create', $tunnelId, '--port-number', [string]$port.Number, '--protocol', 'http', '--description', $port.Description) | Out-Null
        Write-Host "ADDED  $($port.Description)  port $($port.Number)"
    }
}

$hostProcess = Start-Process -FilePath $devTunnel -ArgumentList @('host', $tunnelId) `
    -WorkingDirectory $repoRoot -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $hostOutLog -RedirectStandardError $hostErrLog
[pscustomobject]@{ ProcessId = $hostProcess.Id; TunnelId = $tunnelId } | ConvertTo-Json | Set-Content $hostStateFile -Encoding utf8

$deadline = (Get-Date).AddSeconds(60)
$ready = $false
do {
    if ($hostProcess.HasExited) {
        $details = ((Get-Content $hostOutLog -ErrorAction SilentlyContinue) + (Get-Content $hostErrLog -ErrorAction SilentlyContinue)) -join [Environment]::NewLine
        throw "Dev Tunnel host exited unexpectedly.`n$details"
    }
    $output = Get-Content $hostOutLog -ErrorAction SilentlyContinue
    if (@($output | Select-String -SimpleMatch 'Ready to accept connections').Count -gt 0) {
        $ready = $true
        break
    }
    Start-Sleep -Seconds 2
} while ((Get-Date) -lt $deadline)

if (-not $ready) {
    throw "Timed out while starting Dev Tunnel. Check $hostOutLog and $hostErrLog."
}

$urlLines = @($output | Where-Object { $_ -match 'https://' })
$urlLines | Set-Content (Join-Path $stateRoot 'urls.txt') -Encoding utf8
Write-Host "READY  private Dev Tunnel $tunnelId"
$urlLines | ForEach-Object { Write-Host $_ }
Write-Host 'Remote browsers must sign in with the same Microsoft/GitHub account used by the CLI.'
