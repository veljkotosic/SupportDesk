#!/usr/bin/env bash
set -eo pipefail

SECONDARY_INPUT="${1:-}"

if [[ -z "$SECONDARY_INPUT" ]]; then
  echo "Usage: ./seed.sh <secondary-server-ip-or-es-url>"
  echo "Example: ./seed.sh 192.168.1.50"
  echo "Example: ./seed.sh http://192.168.1.50:9200"
  exit 1
fi

if [[ "$SECONDARY_INPUT" =~ ^https?:// ]]; then
  ES_URL="$SECONDARY_INPUT"
else
  ES_URL="http://${SECONDARY_INPUT}:9200"
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SQL_SEED="$SCRIPT_DIR/seed.sql"
ES_SEED="$SCRIPT_DIR/seed-es.ndjson"

echo "=========================================================="
echo "   SUPPORTDESK STANDALONE SEED RUNNER                     "
echo "=========================================================="
echo " PostgreSQL Target : Localhost (Docker / Host)"
echo " Elasticsearch URL : $ES_URL"
echo " Seed SQL File     : $SQL_SEED"
echo " Seed ES File      : $ES_SEED"
echo "=========================================================="

# 1. Verify files exist
if [[ ! -f "$SQL_SEED" ]]; then
  echo "[!] Error: seed.sql not found at '$SQL_SEED'"
  exit 1
fi

if [[ ! -f "$ES_SEED" ]]; then
  echo "[!] Error: seed-es.ndjson not found at '$ES_SEED'"
  exit 1
fi

# 2. Reseed PostgreSQL
echo -e "\n[1/2] Seeding PostgreSQL from seed.sql..."
PG_USER="${POSTGRES_USER:-sd_veljko}"
PG_DB="${POSTGRES_DB:-sd_db}"
PG_PASSWORD="${POSTGRES_PASSWORD:-sd_tosic}"

PG_CONTAINER=$(docker ps -q -f name=postgres | head -n 1)

if [[ -n "$PG_CONTAINER" ]]; then
  echo "      Found PostgreSQL container: $PG_CONTAINER"
  docker exec -i "$PG_CONTAINER" psql -U "$PG_USER" -d "$PG_DB" < "$SQL_SEED"
elif command -v psql >/dev/null 2>&1; then
  echo "      Connecting via host psql client..."
  PGPASSWORD="$PG_PASSWORD" psql -h localhost -p 5432 -U "$PG_USER" -d "$PG_DB" -f "$SQL_SEED"
else
  echo "[!] Error: Neither running PostgreSQL docker container nor host 'psql' found."
  exit 1
fi
echo "[✔] PostgreSQL database successfully seeded."

# 3. Reseed Elasticsearch
echo -e "\n[2/2] Seeding Elasticsearch at $ES_URL..."
if command -v node >/dev/null 2>&1; then
  node "$SCRIPT_DIR/restore-es.js" "$ES_URL"
elif command -v bun >/dev/null 2>&1; then
  bun run "$SCRIPT_DIR/restore-es.js" "$ES_URL"
else
  echo "      Node/Bun not found on host, running inside node:20-alpine container..."
  docker run --rm --network host -v "$SCRIPT_DIR:/app" -w /app node:20-alpine node /app/restore-es.js "$ES_URL"
fi

echo -e "\n=========================================================="
echo "[✔] BOTH POSTGRESQL & ELASTICSEARCH RESEEDED SUCCESSFULLY!"
echo "=========================================================="
