#!/usr/bin/env bash
set -eo pipefail

POSTGRES_HOST="${1:-localhost}"
ES_URL="${2:-http://localhost:9200}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DATA_DIR="$(cd "$SCRIPT_DIR/../data" && pwd)"
SQL_SEED="$DATA_DIR/seed.sql"

echo "=========================================================="
echo "   SUPPORTDESK COMPLETE DATABASE & SEARCH RESEEDER        "
echo "=========================================================="

if [[ ! -f "$SQL_SEED" ]]; then
  echo "[!] Error: seed.sql not found at '$SQL_SEED'"
  exit 1
fi

# 1. Reseed PostgreSQL
echo -e "\n[1/2] Seeding PostgreSQL from seed.sql..."
if [[ "$POSTGRES_HOST" != "localhost" && "$POSTGRES_HOST" != "127.0.0.1" ]]; then
  echo "      Targeting remote host: $POSTGRES_HOST:5432"
  export PGPASSWORD="${POSTGRES_PASSWORD:-SupportDeskPassword}"
  psql -h "$POSTGRES_HOST" -p 5432 -U SupportDeskUser -d SupportDeskDatabase -f "$SQL_SEED"
else
  echo "      Restoring into local Docker container 'supportdesk-postgres'..."
  docker exec -i supportdesk-postgres psql -U SupportDeskUser -d SupportDeskDatabase < "$SQL_SEED"
fi
echo "[OK] PostgreSQL successfully seeded."

# 2. Reseed Elasticsearch
echo -e "\n[2/2] Seeding Elasticsearch from seed-es.ndjson..."
"$SCRIPT_DIR/restore-es.sh" "$ES_URL"

echo -e "\n[OK] COMPLETE RESEED SUCCESSFUL! BOTH POSTGRESQL & ELASTICSEARCH ARE IN SYNC."
