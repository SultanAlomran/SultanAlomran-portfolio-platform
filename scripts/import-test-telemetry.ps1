param(
    [string]$TelemetryPath = ".\test-results\telemetry.json",
    [string]$ApiBaseUrl = "http://localhost:5100"
)

$resolved = Resolve-Path -LiteralPath $TelemetryPath -ErrorAction Stop
$body = Get-Content -LiteralPath $resolved -Raw
Invoke-RestMethod -Method Post -Uri "$($ApiBaseUrl.TrimEnd('/'))/api/admin/test-analytics/import" -ContentType "application/json" -Body $body
