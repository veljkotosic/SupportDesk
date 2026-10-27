#!/usr/bin/env bash
set -eo pipefail

ES_URL="${1:-http://localhost:9200}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if command -v bun >/dev/null 2>&1; then
  bun run "$SCRIPT_DIR/restore-es.js" "$ES_URL"
elif command -v node >/dev/null 2>&1; then
  node "$SCRIPT_DIR/restore-es.js" "$ES_URL"
else
  echo "[!] Neither 'bun' nor 'node' was found in PATH. Please install either to run the restore script."
  exit 1
fi
