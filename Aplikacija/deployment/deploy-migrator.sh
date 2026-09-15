#!/usr/bin/env bash

# Deploys the short-lived database migrator service.
# Run after PostgreSQL is up and before deploying other services.

set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <registry-ip>"
    exit 1
fi

REGISTRY_IP="$1"

REGISTRY_IP="$REGISTRY_IP" docker stack deploy \
    -c docker-migrator-stack.yml \
    SupportDeskMigrator