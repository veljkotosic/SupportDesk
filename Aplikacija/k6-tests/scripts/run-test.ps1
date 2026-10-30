param (
    [Parameter(Position=0)]
    [string]$Scenario = "scenarios/loadTest.js",

    [string]$Profile = "balanced",
    [string]$BaseUrl = "",
    [int]$VUs = 0,
    [int]$MaxVUs = 0,
    [string]$Duration = "",
    [switch]$NoMonitor
)

$k6RootDir = Join-Path $PSScriptRoot ".."
$resultsDir = Join-Path $k6RootDir "results"
if (-not (Test-Path $resultsDir)) { New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null }

$testTimestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$csvMetrics = Join-Path $resultsDir "resources-$testTimestamp.csv"
$summaryJson = Join-Path $resultsDir "summary-$testTimestamp.json"

# If BaseUrl is remote, disable local process monitor automatically unless explicitly requested
if ($BaseUrl -and ($BaseUrl -notmatch "localhost|127\.0\.0\.1") -and (-not $PSBoundParameters.ContainsKey('NoMonitor'))) {
    $NoMonitor = $true
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "   SUPPORTDESK PERFORMANCE TEST RUNNER                   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Scenario  : $Scenario"
Write-Host " Profile   : $Profile"
if ($BaseUrl)    { Write-Host " BaseUrl   : $BaseUrl" }
if ($VUs -gt 0)    { Write-Host " VUs       : $VUs" }
if ($MaxVUs -gt 0) { Write-Host " Max VUs   : $MaxVUs" }
if ($Scenario -match "loadTest") {
    if ($Duration) { Write-Host " Steady Dur: $Duration (Ramp-up: 1m, Ramp-down: 30s)" } else { Write-Host " Steady Dur: 8m (Ramp-up: 1m, Ramp-down: 30s)" }
} else {
    if ($Duration) { Write-Host " Duration  : $Duration" }
}
Write-Host "==========================================================`n"

$monitorJob = $null
if (-not $NoMonitor) {
    Write-Host "[*] Launching background resource monitor (CPU / RAM)..." -ForegroundColor Yellow
    $monitorScript = Join-Path $PSScriptRoot "monitor-resources.ps1"
    $monitorJob = Start-Job -FilePath $monitorScript -ArgumentList 2, $csvMetrics
}

$k6Args = @("run")
if ($Profile)      { $k6Args += @("-e", "PROFILE=$Profile") }
if ($BaseUrl)      { $k6Args += @("-e", "BASE_URL=$BaseUrl") }

if ($Scenario -match "loadTest") {
    if ($VUs -gt 0)    { $k6Args += @("-e", "VUS=$VUs") }
    if ($MaxVUs -gt 0) { $k6Args += @("-e", "MAX_VUS=$MaxVUs") }
    if ($Duration)     { $k6Args += @("-e", "DURATION=$Duration", "-e", "STEADY_DURATION=$Duration") }
    $k6Args += @("-e", "SUMMARY_PATH=$summaryJson")
} else {
    if ($VUs -gt 0)    { $k6Args += @("--vus", $VUs) }
    if ($MaxVUs -gt 0) { $k6Args += @("-e", "MAX_VUS=$MaxVUs") }
    if ($Duration)     { $k6Args += @("--duration", $Duration) }
    $k6Args += @("--summary-export", $summaryJson)
}

$k6Args += $Scenario

try {
    Write-Host "[*] Starting k6 execution...`n" -ForegroundColor Green
    & k6 $k6Args
} finally {
    if ($monitorJob) {
        Write-Host "`n[*] Stopping background resource monitor..." -ForegroundColor Yellow
        Stop-Job $monitorJob -ErrorAction SilentlyContinue | Out-Null
        $jobOutput = Receive-Job $monitorJob -ErrorAction SilentlyContinue
        Remove-Job $monitorJob -Force -ErrorAction SilentlyContinue | Out-Null
        
        Write-Host "`n==========================================================" -ForegroundColor Cyan
        Write-Host "          SYSTEM RESOURCE USAGE SUMMARY                   " -ForegroundColor Cyan
        Write-Host "==========================================================" -ForegroundColor Cyan
        if (Test-Path $csvMetrics) {
            $data = Import-Csv $csvMetrics -ErrorAction SilentlyContinue
            if ($data -and $data.Count -gt 0) {
                $maxApiCpu = ($data | ForEach-Object { [double]$_.WebApi_CPU_Percent } | Measure-Object -Maximum).Maximum
                $avgApiCpu = [Math]::Round(($data | ForEach-Object { [double]$_.WebApi_CPU_Percent } | Measure-Object -Average).Average, 1)
                $maxApiRam = ($data | ForEach-Object { [double]$_.WebApi_RAM_MB } | Measure-Object -Maximum).Maximum
                
                $maxPgCpu = ($data | ForEach-Object { [double]$_.Postgres_CPU_Percent } | Measure-Object -Maximum).Maximum
                $avgPgCpu = [Math]::Round(($data | ForEach-Object { [double]$_.Postgres_CPU_Percent } | Measure-Object -Average).Average, 1)
                $maxPgRam = ($data | ForEach-Object { [double]$_.Postgres_RAM_MB } | Measure-Object -Maximum).Maximum
                
                $maxRbRam = ($data | ForEach-Object { [double]$_.RabbitMQ_RAM_MB } | Measure-Object -Maximum).Maximum

                Write-Host "  WebApi Peak CPU    : $maxApiCpu % (Average: $avgApiCpu %)" -ForegroundColor Green
                Write-Host "  WebApi Peak Memory : $maxApiRam MB" -ForegroundColor Green
                Write-Host "  Postgres Peak CPU  : $maxPgCpu % (Average: $avgPgCpu %)" -ForegroundColor Blue
                Write-Host "  Postgres Peak RAM  : $maxPgRam MB" -ForegroundColor Blue
                Write-Host "  RabbitMQ Peak RAM  : $maxRbRam MB" -ForegroundColor Yellow
                Write-Host "  Metrics CSV        : $csvMetrics" -ForegroundColor White
            }
        }
        Write-Host "==========================================================`n" -ForegroundColor Cyan
    }
}
