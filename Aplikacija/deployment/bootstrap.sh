#!/usr/bin/env bash

# Starts an image registry and Portainer containers
# Run after setting up the networking/firewall rules etc.

set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <manager-ip>"
    exit 1
fi

MANAGER_IP="$1"
REGISTRY_PORT="5000"

echo "Using manager IP: $MANAGER_IP"

docker run -d \
    --name supportdesk-registry \
    --restart unless-stopped \
    -p "$REGISTRY_PORT:5000" \
    registry:2

echo
echo "Configure Docker to allow:"
echo "    $MANAGER_IP:$REGISTRY_PORT"
echo
echo "Then restart Docker before continuing."

docker volume create portainer_data >/dev/null

docker run -d \
    --name supportdesk-portainer \
    --restart unless-stopped \
    -p 9000:9000 \
    -v /var/run/docker.sock:/var/run/docker.sock \
    -v portainer_data:/data \
    portainer/portainer-ce:latest

echo
echo "Registry:  $MANAGER_IP:$REGISTRY_PORT"
echo "Portainer: http://$MANAGER_IP:9000"