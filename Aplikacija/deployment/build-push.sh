#!/usr/bin/env bash

set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <manager-ip>"
    exit 1
fi

MANAGER_IP="$1"
REGISTRY="$MANAGER_IP:5000"

echo "Using registry: $REGISTRY"

docker build \
    -t "$REGISTRY/supportdesk-nginx:latest" \
    ../front

docker build \
    -t "$REGISTRY/supportdesk-webapi:latest" \
    -f ../dotnet/SupportDesk/SupportDesk.WebApi/Dockerfile \
    ../dotnet/SupportDesk

docker build \
    -t "$REGISTRY/supportdesk-worker:latest" \
    -f ../dotnet/SupportDesk/SupportDesk.Worker/Dockerfile \
    ../dotnet/SupportDesk

docker build \
    -t "$REGISTRY/supportdesk-outbox-processor:latest" \
    -f ../dotnet/SupportDesk/SupportDesk.OutboxProcessor/Dockerfile \
    ../dotnet/SupportDesk

docker build \
    -t "$REGISTRY/supportdesk-migrator:latest" \
    -f ../dotnet/SupportDesk/Migrator.Dockerfile \
    ../dotnet/SupportDesk

docker push "$REGISTRY/supportdesk-nginx:latest"
docker push "$REGISTRY/supportdesk-webapi:latest"
docker push "$REGISTRY/supportdesk-worker:latest"
docker push "$REGISTRY/supportdesk-outbox-processor:latest"
docker push "$REGISTRY/supportdesk-migrator:latest"

echo
echo "Build and push complete."