[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$stateRoot = Join-Path $repoRoot '.dev-environment'
$stateFile = Join-Path $stateRoot 'local-processes.json'

if (-not (Test-Path $stateFile)) {
    Write-Host 'No Portfolio local-process state file was found.'
    exit 0
}

$state = Get-Content -Raw $stateFile | ConvertFrom-Json
$processes = if ($state.PSObject.Properties.Name -contains 'Processes') { $state.Processes } else { $state }
foreach ($processInfo in $processes) {
    $process = Get-Process -Id $processInfo.ProcessId -ErrorAction SilentlyContinue
    if ($process) {
        $termination = Start-Process -FilePath 'taskkill.exe' `
            -ArgumentList @('/PID', [string]$processInfo.ProcessId, '/T', '/F') `
            -NoNewWindow -Wait -PassThru
        if ($termination.ExitCode -ne 0 -and (Get-Process -Id $processInfo.ProcessId -ErrorAction SilentlyContinue)) {
            throw "Could not stop $($processInfo.Name) process tree (PID $($processInfo.ProcessId))."
        }
        Write-Host "STOPPED  $($processInfo.Name)  PID $($processInfo.ProcessId)"
    }
}

Remove-Item -LiteralPath $stateFile -Force
