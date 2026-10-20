#!/usr/bin/env bash
INTERVAL=${1:-2}
OUTPUT_FILE=${2:-""}

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="$(cd "$SCRIPT_DIR/.." && pwd)/results"
mkdir -p "$RESULTS_DIR"

if [[ -z "$OUTPUT_FILE" ]]; then
  TIMESTAMP=$(date +"%Y%m%d-%H%M%S")
  OUTPUT_FILE="$RESULTS_DIR/resource-metrics-$TIMESTAMP.csv"
fi

echo "=========================================================="
echo "   SUPPORTDESK RESOURCE MONITOR (CPU & RAM TRACKER)       "
echo "=========================================================="
echo "Sampling interval : ${INTERVAL}s"
echo "Logging to file   : $OUTPUT_FILE"
echo -e "Press Ctrl+C to stop monitoring.\n"

echo "Timestamp,WebApi_CPU_Percent,WebApi_RAM_MB,Outbox_RAM_MB,Worker_RAM_MB,Postgres_CPU_Percent,Postgres_RAM_MB,RabbitMQ_CPU_Percent,RabbitMQ_RAM_MB,System_Total_CPU_Percent,System_Free_RAM_GB" > "$OUTPUT_FILE"

RUNNING=true
trap 'RUNNING=false' SIGINT SIGTERM

while $RUNNING; do
  NOW=$(date +"%H:%M:%S")

  # 1. Process CPU / RAM for WebApi (if running locally as a process or docker)
  WEB_API_PID=$(pgrep -f "SupportDesk.WebApi" | head -n 1 || true)
  WEB_API_CPU=0
  WEB_API_RAM=0

  if [[ -n "$WEB_API_PID" ]]; then
    if command -v ps &> /dev/null; then
      PROC_STATS=$(ps -p "$WEB_API_PID" -o %cpu,rss --no-headers 2>/dev/null || true)
      if [[ -n "$PROC_STATS" ]]; then
        WEB_API_CPU=$(echo "$PROC_STATS" | awk '{print $1}')
        RAW_RSS_KB=$(echo "$PROC_STATS" | awk '{print $2}')
        if [[ -n "$RAW_RSS_KB" && "$RAW_RSS_KB" =~ ^[0-9]+$ ]]; then
          WEB_API_RAM=$(awk "BEGIN {printf \"%.1f\", $RAW_RSS_KB/1024}")
        fi
      fi
    fi
  fi

  # 2. Docker Containers Stats (Postgres, RabbitMQ, WebApi if containerized)
  PG_CPU=0
  PG_RAM=0
  RABBIT_CPU=0
  RABBIT_RAM=0

  if command -v docker &> /dev/null; then
    DOCKER_STATS=$(docker stats --no-stream --format "{{.Name}},{{.CPUPerc}},{{.MemUsage}}" 2>/dev/null || true)
    if [[ -n "$DOCKER_STATS" ]]; then
      while IFS=, read -r C_NAME C_CPU C_MEM; do
        C_NAME=$(echo "$C_NAME" | tr -d '[:space:]')
        C_CPU_VAL=$(echo "$C_CPU" | tr -d '% ' || echo "0")
        C_MEM_RAW=$(echo "$C_MEM" | awk -F'/' '{print $1}' | tr -d ' ')

        # Convert memory to MB
        C_MEM_MB=0
        if [[ "$C_MEM_RAW" =~ ([0-9.]+)([a-zA-Z]+) ]]; then
          NUM="${BASH_REMATCH[1]}"
          UNIT=$(echo "${BASH_REMATCH[2]}" | tr '[:lower:]' '[:upper:]')
          case "$UNIT" in
            GIB|GB) C_MEM_MB=$(awk "BEGIN {printf \"%.1f\", $NUM * 1024}") ;;
            MIB|MB) C_MEM_MB=$(awk "BEGIN {printf \"%.1f\", $NUM}") ;;
            KIB|KB) C_MEM_MB=$(awk "BEGIN {printf \"%.1f\", $NUM / 1024}") ;;
          esac
        fi

        if [[ "$C_NAME" =~ postgres ]]; then
          PG_CPU="$C_CPU_VAL"
          PG_RAM="$C_MEM_MB"
        elif [[ "$C_NAME" =~ rabbitmq ]]; then
          RABBIT_CPU="$C_CPU_VAL"
          RABBIT_RAM="$C_MEM_MB"
        elif [[ "$C_NAME" =~ webapi && "$WEB_API_CPU" == "0" ]]; then
          WEB_API_CPU="$C_CPU_VAL"
          WEB_API_RAM="$C_MEM_MB"
        fi
      done <<< "$DOCKER_STATS"
    fi
  fi

  # 3. Free System RAM
  FREE_RAM_GB=0
  if command -v free &> /dev/null; then
    FREE_MEM_MB=$(free -m | awk '/^Mem:/{print $7}')
    if [[ -n "$FREE_MEM_MB" ]]; then
      FREE_RAM_GB=$(awk "BEGIN {printf \"%.2f\", $FREE_MEM_MB / 1024}")
    fi
  fi

  echo "[$NOW] WebApi: ${WEB_API_CPU}% CPU / ${WEB_API_RAM} MB | Postgres: ${PG_CPU}% CPU / ${PG_RAM} MB | RabbitMQ: ${RABBIT_CPU}% CPU / ${RABBIT_RAM} MB | Free RAM: ${FREE_RAM_GB}GB"
  echo "$NOW,$WEB_API_CPU,$WEB_API_RAM,0,0,$PG_CPU,$PG_RAM,$RABBIT_CPU,$RABBIT_RAM,0,$FREE_RAM_GB" >> "$OUTPUT_FILE"

  sleep "$INTERVAL"
done

echo ""
echo "=========================================================="
echo "               RESOURCE MONITOR STOPPED                   "
echo "=========================================================="
echo "  Results saved to : $OUTPUT_FILE"
echo "=========================================================="
