param(
    [string]$ElasticsearchUrl = "http://localhost:9200"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Prefer bun if available, fallback to node
if (Get-Command bun -ErrorAction SilentlyContinue) {
    bun run "$ScriptDir\restore-es.js" $ElasticsearchUrl
} elseif (Get-Command node -ErrorAction SilentlyContinue) {
    node "$ScriptDir\restore-es.js" $ElasticsearchUrl
} else {
    Write-Error "Neither 'bun' nor 'node' was found in PATH. Please install either to run the restore script."
}
