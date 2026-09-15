#!/usr/bin/env bash

# Deploys infrastructure stack
# Includes Postgres, RabbitMQ and Seq
# Includes backend virtual network

set -euo pipefail

docker stack deploy \
    -c docker-infra-stack.yml \
    SupportDeskInfra