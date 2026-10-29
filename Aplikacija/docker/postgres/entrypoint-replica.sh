#!/usr/bin/env bash
set -eo pipefail

PRIMARY_HOST="${PRIMARY_HOST:-postgres}"
PRIMARY_PORT="${PRIMARY_PORT:-5432}"
REPL_USER="${REPLICATION_USER:-replicator}"
REPL_PASS="${REPLICATION_PASSWORD:-replicator_password}"
REPL_SLOT="${REPLICATION_SLOT:-replica_slot}"

mkdir -p "$PGDATA"
chown -R postgres:postgres "$PGDATA"
chmod 700 "$PGDATA"

# If FORCE_RESYNC is set or if PG_VERSION is missing, run fresh pg_basebackup
if [ "${FORCE_RESYNC:-0}" = "1" ] || [ ! -s "$PGDATA/PG_VERSION" ]; then
    echo "=== Standby Replica: Waiting for primary at ${PRIMARY_HOST}:${PRIMARY_PORT}... ==="
    until PGPASSWORD="$POSTGRES_PASSWORD" gosu postgres pg_isready -h "$PRIMARY_HOST" -p "$PRIMARY_PORT" -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
        sleep 2
    done

    echo "=== Running pg_basebackup from primary database with replication slot ${REPL_SLOT}... ==="
    rm -rf "${PGDATA:?}"/*

    PGPASSWORD="$REPL_PASS" gosu postgres pg_basebackup \
        -h "$PRIMARY_HOST" \
        -p "$PRIMARY_PORT" \
        -U "$REPL_USER" \
        -D "$PGDATA" \
        -Fp -Xs -R -c fast -v \
        -S "$REPL_SLOT"

    chown -R postgres:postgres "$PGDATA"
    chmod 700 "$PGDATA"
    echo "=== Base backup completed with slot ${REPL_SLOT}. Standby configuration active. ==="
fi

echo "=== Starting PostgreSQL Read Replica in Standby Mode ==="
exec docker-entrypoint.sh "$@"
