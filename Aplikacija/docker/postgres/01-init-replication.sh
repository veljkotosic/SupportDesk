#!/usr/bin/env bash
set -eo pipefail

echo "=== Initializing PostgreSQL Primary Replication Role ==="

REPL_USER="${REPLICATION_USER:-replicator}"
REPL_PASS="${REPLICATION_PASSWORD:-replicator_password}"
REPL_SLOT="${REPLICATION_SLOT:-replica_slot}"

# 1. Create replication role if it does not already exist
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    DO \$\$
    BEGIN
        IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '$REPL_USER') THEN
            CREATE ROLE $REPL_USER WITH REPLICATION LOGIN PASSWORD '$REPL_PASS';
        END IF;

        IF NOT EXISTS (SELECT 1 FROM pg_replication_slots WHERE slot_name = '$REPL_SLOT') THEN
            PERFORM pg_create_physical_replication_slot('$REPL_SLOT');
        END IF;
    END
    \$\$;
EOSQL

# 2. Append replication entry to pg_hba.conf if not already present
if ! grep -q "host replication $REPL_USER" "$PGDATA/pg_hba.conf"; then
    echo "host replication $REPL_USER all scram-sha-256" >> "$PGDATA/pg_hba.conf"
fi

pg_ctl reload
echo "=== Replication role, replication slot, and pg_hba.conf configured successfully ==="
