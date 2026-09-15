#!/usr/bin/env bash

# Deploys core application services
# Make sure migrations are applied before running

set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <registry-ip>"
    exit 1
fi

REGISTRY_IP="$1"

REGISTRY_IP="$REGISTRY_IP" docker stack deploy \
    -c docker-service-stack.yml \
    SupportDesk