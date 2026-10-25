#!/usr/bin/env bash
set -eo pipefail

REPL_USER="${REPLICATION_USER:-replicator}"
REPL_PASS="${REPLICATION_PASSWORD:-replicator_password}"
REPL_SLOT="${REPLICATION_SLOT:-replica_slot}"

# Background daemon ensuring replication role, slot, and pg_hba.conf exist even on existing/restored volumes
(
    export PGPASSWORD="$POSTGRES_PASSWORD"
    until pg_isready -h 127.0.0.1 -p 5432 -U "$POSTGRES_USER" -d "$POSTGRES_DB" >/dev/null 2>&1; do
        sleep 1
    done

    # Ensure pg_hba.conf allows replication from any host
    if [ -f "$PGDATA/pg_hba.conf" ]; then
        if ! grep -q "host replication $REPL_USER" "$PGDATA/pg_hba.conf"; then
            echo "host replication $REPL_USER all scram-sha-256" >> "$PGDATA/pg_hba.conf"
            psql -h 127.0.0.1 -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -c "SELECT pg_reload_conf();" >/dev/null 2>&1 || true
        fi
    fi

    # Ensure replication role and physical replication slot exist
    psql -h 127.0.0.1 -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL >/dev/null 2>&1
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
    echo "=== Primary: Replication role and slot verified successfully ==="
) &

exec docker-entrypoint.sh "$@"
