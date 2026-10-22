# SupportDesk k6 Performance Testing

Performance and load testing suite for the SupportDesk application. Simulates user workflows for Customers, Support Agents, and Organization Admins, including HTTP REST endpoints and SignalR WebSocket connections.

---

## Contents
- [Prerequisites](#prerequisites)
- [Database Seeding](#database-seeding)
- [Running Tests](#running-tests)
- [Script Parameters](#script-parameters)
- [Scenarios](#scenarios)
- [Workload Profiles](#workload-profiles)
- [Remote Execution (Docker Swarm)](#remote-execution-docker-swarm)
- [Resource Monitoring](#resource-monitoring)

---

## Prerequisites

- [k6](https://k6.io/) installed and available in PATH.
- SupportDesk Web API running locally or on a remote host.
- PostgreSQL database with schema migrations applied and test data seeded.

---

## Database Seeding

The test suite requires pre-existing accounts to authenticate. 

### Default Credentials
- **Password**: `Password123!` (all users)
- **Customers**: `customer1@test.com` to `customer10000@test.com`
- **Organization Admins**: `admin1@org1.com` to `admin273@org273.com`
- **Support Agents**: Loaded from `data/agents.json` (fallback list in `data/users.js`)

### Seeding with TestDataGenerator

Run the data generator from the `SupportDesk.TestDataGenerator` project directory:

```bash
# Local database (uses DB_CONN_STRING from .env or defaults)
dotnet run --project SupportDesk.TestDataGenerator

# Remote database / Docker Swarm
DB_CONN_STRING="Host=<host>;Port=5432;Database=SupportDeskDatabase;Username=SupportDeskUser;Password=SupportDeskPassword;" \
dotnet run --project SupportDesk.TestDataGenerator
```

In PowerShell:
```powershell
$env:DB_CONN_STRING="Host=<host>;Port=5432;Database=SupportDeskDatabase;Username=SupportDeskUser;Password=SupportDeskPassword;"
dotnet run --project SupportDesk.TestDataGenerator
```

### Seeding from SQL Dump

Alternatively, import the pre-generated database dump:

```bash
# Docker local
docker exec -i supportdesk-postgres psql -U SupportDeskUser -d SupportDeskDatabase < data/seed.sql

# Remote PostgreSQL
psql -h <host> -p 5432 -U SupportDeskUser -d SupportDeskDatabase -f data/seed.sql
```

---

## Running Tests

Scripts are provided for PowerShell (`run-test.ps1`) and Bash (`run-test.sh`).

### PowerShell

```powershell
# Breakpoint test
.\scripts\run-test.ps1 -Scenario "scenarios/breakpointTest.js" -Profile "balanced" -MaxVUs 3000

# Load test
.\scripts\run-test.ps1 -Scenario "scenarios/loadTest.js" -Profile "balanced" -MaxVUs 500

# Spike test
.\scripts\run-test.ps1 -Scenario "scenarios/spikeTest.js" -Profile "writeHeavy" -MaxVUs 500

# Target remote host
.\scripts\run-test.ps1 -Scenario "scenarios/loadTest.js" -BaseUrl "http://192.168.1.100:5011" -MaxVUs 1000

# Fixed VUs and duration
.\scripts\run-test.ps1 -Scenario "scenarios/workloadSuite.js" -Profile "readHeavy" -VUs 100 -Duration "5m"
```

### Bash

```bash
chmod +x scripts/run-test.sh scripts/monitor-resources.sh

# Breakpoint test
./scripts/run-test.sh -s scenarios/breakpointTest.js -p balanced -m 3000

# Load test
./scripts/run-test.sh -s scenarios/loadTest.js -p balanced -m 500

# Spike test
./scripts/run-test.sh -s scenarios/spikeTest.js -p writeHeavy -m 500

# Target remote host
./scripts/run-test.sh -s scenarios/loadTest.js -b "http://192.168.1.100:5011" -m 1000

# Fixed VUs and duration
./scripts/run-test.sh -s scenarios/workloadSuite.js -p readHeavy -u 100 -d "5m"
```

---

## Script Parameters

| PowerShell | Bash | Default | Description |
|---|---|---|---|
| `-Scenario` | `-s`, `--scenario` | `scenarios/loadTest.js` | Path to the k6 scenario script. |
| `-Profile` | `-p`, `--profile` | `balanced` | Workload profile to execute. |
| `-BaseUrl` | `-b`, `--base-url` | `http://localhost:5011` | Root URL of the API. Configures both HTTP and WebSocket endpoints. |
| `-MaxVUs` | `-m`, `--max-vus` | Scenario default | Peak Virtual Users for ramping stages. |
| `-VUs` | `-u`, `--vus` | `0` | Overrides scenario stages with a constant VU count. |
| `-Duration` | `-d`, `--duration` | `""` | Overrides scenario stages with a fixed duration (e.g. `10m`, `30s`). |
| `-NoMonitor` | `--no-monitor` | `false` | Disables background resource monitoring. Enabled automatically when `-BaseUrl` is not localhost. |

---

## Scenarios

- **Breakpoint Test (`scenarios/breakpointTest.js`)**: Ramps VUs progressively (5% to 100% of `MaxVUs` over 8 stages). Contains threshold aborts (`abortOnFail: true`) if p95 latency exceeds 1.5s or error rate exceeds 5% after the 1-minute warmup. Used to find the system's concurrency limit.
- **Load Test (`scenarios/loadTest.js`)**: 12-minute run (2m ramp-up, 8m steady-state at target `MaxVUs`, 2m ramp-down). Used to measure steady-state performance and resource usage.
- **Spike Test (`scenarios/spikeTest.js`)**: Runs 30s at 5 VUs, surges to `MaxVUs` over 10s, sustains for 1m, drops back in 10s, and observes recovery for 1m.
- **Workload Suite (`scenarios/workloadSuite.js`)**: Executes the configured workload profile without predefined stage schedules. Used with `-VUs` and `-Duration`.

---

## Workload Profiles

Profiles determine the proportion of read vs. write operations:

| Profile | Read | Write | Typical Use |
|---|---|---|---|
| `balanced` | 50% | 50% | General usage. |
| `readDominant_75_25` | 75% | 25% | Read-heavy browsing with regular updates. |
| `writeDominant_25_75` | 25% | 75% | Active ticket creation and chat messaging. |
| `readHeavy` | 90% | 10% | Ticket and FAQ browsing, dashboard monitoring. |
| `writeHeavy` | 10% | 90% | Write-intensive mutation and outbox testing. |

---

## Remote Execution (Docker Swarm)

When running k6 from an external machine against Docker Swarm:

1. Provide the Swarm entry point using `-BaseUrl` / `-b`:
   ```bash
   ./scripts/run-test.sh -s scenarios/loadTest.js -b "http://<swarm-ip>:<port>" -m 1000
   ```
2. The runner automatically disables local machine resource monitoring when `-BaseUrl` points to a non-localhost address.
3. WebSockets connect directly to `/hubs/...` without HTTP negotiation, so no sticky sessions are required on the Swarm load balancer.
4. If testing at high concurrency (>1,000 VUs), ensure the Web API connection string in Swarm configures sufficient connection pool size (e.g. `Maximum Pool Size=300;`).

---

## Resource Monitoring

Hardware monitoring scripts record CPU and RAM metrics every 2 seconds:
- Web API process CPU % and RAM (MB)
- Docker containers (PostgreSQL, RabbitMQ) CPU % and RAM (MB)
- Host free physical memory (GB)

Results are written to `results/resources-<timestamp>.csv`.

### Running Monitor Standalone

PowerShell:
```powershell
.\scripts\monitor-resources.ps1 -IntervalSeconds 2 -OutputFile "results/metrics.csv"
```

Bash:
```bash
./scripts/monitor-resources.sh 2 "results/metrics.csv"
```
