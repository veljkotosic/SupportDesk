param (
    [int]$IntervalSeconds = 2,
    [string]$OutputFile = ""
)

if ([string]::IsNullOrWhiteSpace($OutputFile)) {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $resultsDir = Join-Path $PSScriptRoot "..\results"
    if (-not (Test-Path $resultsDir)) { New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null }
    $OutputFile = Join-Path $resultsDir "resource-metrics-$timestamp.csv"
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   SUPPORTDESK RESOURCE MONITOR (CPU & RAM TRACKER)       " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Sampling interval : $IntervalSeconds s"
Write-Host "Logging to file   : $OutputFile"
Write-Host "Press Ctrl+C to stop monitoring.`n"

"Timestamp,WebApi_CPU_Percent,WebApi_RAM_MB,Outbox_RAM_MB,Worker_RAM_MB,Postgres_CPU_Percent,Postgres_RAM_MB,RabbitMQ_CPU_Percent,RabbitMQ_RAM_MB,System_Total_CPU_Percent,System_Free_RAM_GB" | Out-File -FilePath $OutputFile -Encoding utf8

$samples = [System.Collections.Generic.List[PSCustomObject]]::new()

try {
    while ($true) {
        $now = Get-Date -Format "HH:mm:ss"

        $webApiProc = Get-Process -Name "*SupportDesk.WebApi*" -ErrorAction SilentlyContinue | Select-Object -First 1
        $outboxProc = Get-Process -Name "*SupportDesk.OutboxProcessor*" -ErrorAction SilentlyContinue | Select-Object -First 1
        $workerProc = Get-Process -Name "*SupportDesk.Worker*" -ErrorAction SilentlyContinue | Select-Object -First 1

        $webApiRamMb = if ($webApiProc) { [Math]::Round($webApiProc.WorkingSet64 / 1MB, 1) } else { 0 }
        $outboxRamMb = if ($outboxProc) { [Math]::Round($outboxProc.WorkingSet64 / 1MB, 1) } else { 0 }
        $workerRamMb = if ($workerProc) { [Math]::Round($workerProc.WorkingSet64 / 1MB, 1) } else { 0 }

        $webApiCpu = 0
        if ($webApiProc) {
            $cpu1 = $webApiProc.TotalProcessorTime.TotalMilliseconds
            $time1 = [System.Diagnostics.Stopwatch]::GetTimestamp()
            Start-Sleep -Milliseconds 400
            $webApiProc.Refresh()
            $cpu2 = $webApiProc.TotalProcessorTime.TotalMilliseconds
            $time2 = [System.Diagnostics.Stopwatch]::GetTimestamp()

            $elapsedMs = ($time2 - $time1) / [System.Diagnostics.Stopwatch]::Frequency * 1000
            if ($elapsedMs -gt 0) {
                $webApiCpu = [Math]::Round((($cpu2 - $cpu1) / ($elapsedMs * [Environment]::ProcessorCount)) * 100, 1)
            }
        } else {
            Start-Sleep -Milliseconds 400
        }

        # 2. Docker Containers (Postgres, RabbitMQ)
        $postgresCpu = 0; $postgresRam = 0
        $rabbitCpu = 0; $rabbitRam = 0

        $dockerStats = docker stats --no-stream --format "{{.Name}},{{.CPUPerc}},{{.MemUsage}}" 2>$null
        if ($dockerStats) {
            foreach ($line in $dockerStats) {
                $parts = $line -split ","
                if ($parts.Length -ge 3) {
                    $cName = $parts[0].Trim()
                    $cCpu = [double]($parts[1] -replace '%', '').Trim()
                    $cMemRaw = $parts[2] -split "/"
                    $cMemVal = $cMemRaw[0].Trim()
                    $cMemMb = 0
                    if ($cMemVal -match '([0-9.]+)\s*([a-zA-Z]+)') {
                        $num = [double]$Matches[1]
                        $unit = $Matches[2].ToUpper()
                        if ($unit -eq 'MIB' -or $unit -eq 'MB') { $cMemMb = $num }
                        elseif ($unit -eq 'GIB' -or $unit -eq 'GB') { $cMemMb = $num * 1024 }
                        elseif ($unit -eq 'KIB' -or $unit -eq 'KB') { $cMemMb = $num / 1024 }
                    }

                    if ($cName -like "*postgres*") {
                        $postgresCpu = $cCpu
                        $postgresRam = [Math]::Round($cMemMb, 1)
                    } elseif ($cName -like "*rabbitmq*") {
                        $rabbitCpu = $cCpu
                        $rabbitRam = [Math]::Round($cMemMb, 1)
                    }
                }
            }
        }

        $osInfo = Get-CimInstance Win32_OperatingSystem -ErrorAction SilentlyContinue
        $freeRamGb = if ($osInfo) { [Math]::Round($osInfo.FreePhysicalMemory / 1MB, 2) } else { 0 }

        Write-Host "[$now] " -NoNewline -ForegroundColor DarkGray
        Write-Host "WebApi: " -NoNewline -ForegroundColor Green
        Write-Host "$webApiCpu% CPU / $webApiRamMb MB | " -NoNewline
        Write-Host "Postgres: " -NoNewline -ForegroundColor Blue
        Write-Host "$postgresCpu% CPU / $postgresRam MB | " -NoNewline
        Write-Host "RabbitMQ: " -NoNewline -ForegroundColor Yellow
        Write-Host "$rabbitCpu% CPU / $rabbitRam MB | " -NoNewline
        Write-Host "Free RAM: ${freeRamGb}GB" -ForegroundColor DarkCyan

        "$now,$webApiCpu,$webApiRamMb,$outboxRamMb,$workerRamMb,$postgresCpu,$postgresRam,$rabbitCpu,$rabbitRam,0,$freeRamGb" | Out-File -FilePath $OutputFile -Append -Encoding utf8

        $samples.Add([PSCustomObject]@{
            WebApiCpu = $webApiCpu
            WebApiRam = $webApiRamMb
            PostgresCpu = $postgresCpu
            PostgresRam = $postgresRam
            RabbitCpu = $rabbitCpu
            RabbitRam = $rabbitRam
        })

        $remainingSleep = [Math]::Max(100, ($IntervalSeconds * 1000) - 450)
        Start-Sleep -Milliseconds $remainingSleep
    }
} finally {
    Write-Host "`n==========================================================" -ForegroundColor Cyan
    Write-Host "               RESOURCE MONITOR SUMMARY                   " -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Cyan
    if ($samples.Count -gt 0) {
        $avgApiCpu = [Math]::Round(($samples | Measure-Object -Property WebApiCpu -Average).Average, 1)
        $maxApiCpu = ($samples | Measure-Object -Property WebApiCpu -Maximum).Maximum
        $maxApiRam = ($samples | Measure-Object -Property WebApiRam -Maximum).Maximum

        $avgPgCpu = [Math]::Round(($samples | Measure-Object -Property PostgresCpu -Average).Average, 1)
        $maxPgCpu = ($samples | Measure-Object -Property PostgresCpu -Maximum).Maximum
        $maxPgRam = ($samples | Measure-Object -Property PostgresRam -Maximum).Maximum

        $maxRbRam = ($samples | Measure-Object -Property RabbitRam -Maximum).Maximum

        Write-Host "  WebApi Peak CPU    : $maxApiCpu % (Avg: $avgApiCpu %)" -ForegroundColor Green
        Write-Host "  WebApi Peak Memory : $maxApiRam MB" -ForegroundColor Green
        Write-Host "  Postgres Peak CPU  : $maxPgCpu % (Avg: $avgPgCpu %)" -ForegroundColor Blue
        Write-Host "  Postgres Peak RAM  : $maxPgRam MB" -ForegroundColor Blue
        Write-Host "  RabbitMQ Peak RAM  : $maxRbRam MB" -ForegroundColor Yellow
        Write-Host "  Results saved to   : $OutputFile" -ForegroundColor White
    }
    Write-Host "==========================================================`n" -ForegroundColor Cyan
}
