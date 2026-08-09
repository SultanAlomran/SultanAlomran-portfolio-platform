[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$stateRoot = Join-Path $repoRoot '.dev-environment'
$logRoot = Join-Path $stateRoot 'logs'
$stateFile = Join-Path $stateRoot 'local-processes.json'
$services = @(
    @{ Name = 'Portfolio.Api'; Port = 5100; Url = 'http://localhost:5100/health' },
    @{ Name = 'Portfolio.Web'; Port = 4200; Url = 'http://localhost:4200/' },
    @{ Name = 'Portfolio.Admin'; Port = 4300; Url = 'http://localhost:4300/' }
)

function Test-PortListening([int]$Port) {
    return [bool](Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue)
}

function Stop-StartedProcesses([object[]]$Processes) {
    foreach ($processInfo in $Processes) {
        $termination = Start-Process -FilePath 'taskkill.exe' `
            -ArgumentList @('/PID', [string]$processInfo.ProcessId, '/T', '/F') `
            -NoNewWindow -Wait -PassThru
        if ($termination.ExitCode -ne 0 -and (Get-Process -Id $processInfo.ProcessId -ErrorAction SilentlyContinue)) {
            throw "Could not stop $($processInfo.Name) process tree (PID $($processInfo.ProcessId))."
        }
    }
}

foreach ($service in $services) {
    if (Test-PortListening $service.Port) {
        throw "$($service.Name) cannot start because port $($service.Port) is already in use. Run scripts/stop-local.ps1 or stop the owning process first."
    }
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
$started = @()

try {
    $api = Start-Process -FilePath 'dotnet' `
        -ArgumentList @('run', '--project', (Join-Path $repoRoot 'src/Portfolio.Api/Portfolio.Api.csproj'), '--launch-profile', 'http') `
        -WorkingDirectory $repoRoot -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $logRoot 'api.out.log') `
        -RedirectStandardError (Join-Path $logRoot 'api.err.log')
    $started += [pscustomobject]@{ Name = 'Portfolio.Api'; ProcessId = $api.Id; Port = 5100 }

    $web = Start-Process -FilePath 'npm.cmd' `
        -ArgumentList @('--prefix', (Join-Path $repoRoot 'src/Portfolio.Web'), 'run', 'start:tunnel') `
        -WorkingDirectory $repoRoot -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $logRoot 'web.out.log') `
        -RedirectStandardError (Join-Path $logRoot 'web.err.log')
    $started += [pscustomobject]@{ Name = 'Portfolio.Web'; ProcessId = $web.Id; Port = 4200 }

    $admin = Start-Process -FilePath 'npm.cmd' `
        -ArgumentList @('--prefix', (Join-Path $repoRoot 'src/Portfolio.Admin'), 'run', 'start:tunnel') `
        -WorkingDirectory $repoRoot -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $logRoot 'admin.out.log') `
        -RedirectStandardError (Join-Path $logRoot 'admin.err.log')
    $started += [pscustomobject]@{ Name = 'Portfolio.Admin'; ProcessId = $admin.Id; Port = 4300 }

    [pscustomobject]@{ Processes = @($started) } | ConvertTo-Json -Depth 3 | Set-Content -Path $stateFile -Encoding utf8

    $deadline = (Get-Date).AddSeconds(90)
    do {
        $pending = @($services | Where-Object { -not (Test-PortListening $_.Port) })
        if ($pending.Count -eq 0) { break }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)

    if ($pending.Count -gt 0) {
        throw "Timed out waiting for: $($pending.Name -join ', '). Check $logRoot."
    }

    foreach ($service in $services) {
        $response = Invoke-WebRequest -Uri $service.Url -UseBasicParsing -TimeoutSec 15
        Write-Host "READY  $($service.Name)  $($service.Url)  HTTP $($response.StatusCode)"
    }

    Write-Host "Logs: $logRoot"
}
catch {
    Stop-StartedProcesses $started
    throw
}
