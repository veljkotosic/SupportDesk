### SupportDesk Production & Staging Deployment Guide

This directory contains configuration files and shell scripts for deploying the **SupportDesk** platform to a **Docker Swarm** cluster.

---

### System Architecture & Stacks

The deployment consists of three Docker Swarm stacks:

1. **`SupportDeskInfra` (`docker-infra-stack.yml`)**:
    * **PostgreSQL 18**: Main relational database.
    * **RabbitMQ 3 (with Management)**: Message broker and real-time event bus.
    * **Seq**: Centralized structured logging and OpenTelemetry ingestion endpoint.
    * **Network**: Creates the overlay network `SupportDesk_backend`.

2. **`SupportDeskMigrator` (`docker-migrator-stack.yml`)**:
    * Short-lived container that runs Entity Framework Core database migrations on startup.

3. **`SupportDesk` (`docker-service-stack.yml`)**:
    * **WebApi**: ASP.NET Core REST API & SignalR hub backend (`replicas: 2`).
    * **Worker**: Background queue consumer and processing worker (`replicas: 2`).
    * **Outbox Processor**: Transactional outbox event dispatcher (`replicas: 1`).
    * **Nginx Frontend**: Reverse proxy serving Vue 3 static assets and proxying API/SignalR traffic (`port 80`).
    * **Network**: Creates overlay network `SupportDesk_frontend` and attaches to `SupportDesk_backend`.

---

### Prerequisites

1. **Docker Engine & Docker Swarm**:
   Docker Swarm must be initialized on the manager node:
   ```bash
   docker swarm init
   ```

2. **Node Labeling**:
   Infrastructure and single-instance services are constrained to run on nodes with the `role=manager` label:
   ```bash
   docker node update --label-add role=manager <NODE-HOSTNAME>
   ```

3. **Insecure Registry Setup**:
   Because the local registry runs over plain HTTP on port `5000`, configure Docker on all cluster nodes to allow insecure registry access:
   Edit `/etc/docker/daemon.json`:
   ```json
   {
     "insecure-registries": ["<MANAGER-IP>:5000"]
   }
   ```
   Then reload and restart the Docker daemon:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl restart docker
   ```

---

### Configuration

Create your `.env` file from `.env.example`:

```bash
cp .env.example .env
```

Configure the environment variables in `.env`:

| Variable | Description |
| :--- | :--- |
| `POSTGRES_DB` | PostgreSQL database name |
| `POSTGRES_USER` | PostgreSQL user |
| `POSTGRES_PASSWORD` | PostgreSQL password |
| `DB_CONN_STRING` | Connection string: `Host=postgres;Port=5432;Database=...;Username=...;Password=...` |
| `RABBITMQ_AMQP_URI` | Connection URI: `amqp://guest:guest@rabbitmq:5672/` |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry Seq endpoint: `http://seq:5341/ingest/otlp` |
| `JWT_ISSUER` | JWT token issuer (e.g. `SupportDesk`) |
| `JWT_AUDIENCE` | JWT token audience (e.g. `SupportDeskWebApi`) |
| `JWT_KEY` | Secret key for signing JWT tokens |
| `JWT_EXPIRATION_MINUTES` | Access token lifespan in minutes |
| `JWT_CUSTOMER_REFRESH_TOKEN_EXPIRATION_DAYS` | Customer refresh token lifespan in days |
| `JWT_ORGANIZATION_REFRESH_TOKEN_EXPIRATION_DAYS` | Organization user refresh token lifespan in days |

---

### Step-by-Step Deployment Procedure

Run all commands from the `Aplikacija/deployment` directory.

#### Step 1: Bootstrap Registry & Portainer

Starts the private Docker registry on port `5000` and Portainer on port `9000`:

```bash
chmod +x *.sh
./bootstrap.sh <MANAGER-IP>
```

#### Step 2: Build and Push Application Images

Builds all backend Dockerfiles, Migrator, and Frontend images, then pushes them to `<MANAGER-IP>:5000`:

```bash
./build-push.sh <MANAGER-IP>
```

#### Step 3: Deploy Infrastructure Stack

Deploys PostgreSQL, RabbitMQ, Seq, and the `SupportDesk_backend` overlay network:

```bash
./deploy-infra.sh
```

> **Note**: Wait ~15–20 seconds for PostgreSQL and RabbitMQ health checks to pass before running the next step. You can check service health with `docker service ls`.

#### Step 4: Apply Database Migrations

Runs the database migrator container against PostgreSQL:

```bash
./deploy-migrator.sh <MANAGER-IP>
```

Check the migration status and logs:
```bash
docker service logs -f SupportDeskMigrator_migrator
```

Once migrations have finished successfully, remove the migrator stack:
```bash
docker stack rm SupportDeskMigrator
```

#### Step 5: Deploy Application Services

Deploys the Web API, background workers, outbox processor, and Nginx frontend:

```bash
./deploy-services.sh <MANAGER-IP>
```

---

### Service URLs & Endpoints

| Service | URL / Port | Credentials / Notes |
| :--- | :--- | :--- |
| **Web Application (Frontend & API)** | `http://<MANAGER-IP>` (Port 80) | Nginx reverse proxy |
| **Portainer UI** | `http://<MANAGER-IP>:9000` | Setup admin password on first run |
| **Seq Structured Logging** | `http://<MANAGER-IP>:5341` | Real-time structured log dashboard |
| **RabbitMQ Management** | `http://<MANAGER-IP>:15672` | Default: `guest` / `guest` |
| **PostgreSQL Database** | `<MANAGER-IP>:5432` | Configured in `.env` |

---

### Maintenance & Management

#### View Running Services
```bash
docker stack services SupportDesk
docker stack services SupportDeskInfra
```

#### View Service Logs
```bash
docker service logs -f SupportDesk_webapi
docker service logs -f SupportDesk_worker
docker service logs -f SupportDesk_outbox-processor
docker service logs -f SupportDesk_nginx
```

#### Updating Services (Zero-Downtime Rolling Update)
To deploy new application changes:
1. Re-run `./build-push.sh <MANAGER-IP>`
2. Update the stack:
   ```bash
   REGISTRY_IP="<MANAGER-IP>" docker stack deploy -c docker-service-stack.yml SupportDesk
   ```

#### Teardown Stacks
To take down all application services and infrastructure:
```bash
docker stack rm SupportDesk
docker stack rm SupportDeskInfra