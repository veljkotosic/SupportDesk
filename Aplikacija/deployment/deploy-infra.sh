#!/usr/bin/env bash

# Deploys infrastructure stack
# Includes Postgres Primary, Postgres Replica, RabbitMQ and Seq
# Includes backend virtual network

set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <registry-ip>"
    exit 1
fi

REGISTRY_IP="$1"

REGISTRY_IP="$REGISTRY_IP" docker stack deploy \
    -c docker-infra-stack.yml \
    SupportDeskInfra