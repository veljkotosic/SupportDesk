param(
    [string]$PostgresHost = "localhost",
    [int]$PostgresPort = 5432,
    [string]$ElasticsearchUrl = "http://localhost:9200",
    [switch]$Remote
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DataDir = Join-Path (Split-Path -Parent $ScriptDir) "data"
$SqlSeed = Join-Path $DataDir "seed.sql"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   SUPPORTDESK COMPLETE DATABASE & SEARCH RESEEDER        " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

if (-not (Test-Path $SqlSeed)) {
    Write-Error "[!] Error: seed.sql not found at '$SqlSeed'"
}

# 1. Reseed PostgreSQL
Write-Host "`n[1/2] Seeding PostgreSQL from seed.sql..." -ForegroundColor Green
if ($Remote -or ($PostgresHost -ne "localhost" -and $PostgresHost -ne "127.0.0.1")) {
    Write-Host "      Targeting remote host: $PostgresHost`:$PostgresPort"
    $env:PGPASSWORD = if ($env:POSTGRES_PASSWORD) { $env:POSTGRES_PASSWORD } else { "SupportDeskPassword" }
    psql -h $PostgresHost -p $PostgresPort -U SupportDeskUser -d SupportDeskDatabase -f "$SqlSeed"
} else {
    Write-Host "      Restoring into local Docker container 'supportdesk-postgres'..."
    cmd.exe /c "docker exec -i supportdesk-postgres psql -U SupportDeskUser -d SupportDeskDatabase < `"$SqlSeed`""
}
Write-Host "[OK] PostgreSQL successfully seeded." -ForegroundColor Green

# 2. Reseed Elasticsearch
Write-Host "`n[2/2] Seeding Elasticsearch from seed-es.ndjson..." -ForegroundColor Green
& "$ScriptDir\restore-es.ps1" -ElasticsearchUrl $ElasticsearchUrl

Write-Host "`n[OK] COMPLETE RESEED SUCCESSFUL! BOTH POSTGRESQL & ELASTICSEARCH ARE IN SYNC." -ForegroundColor Green
