#!/usr/bin/env bash
set -eo pipefail

SCENARIO="scenarios/loadTest.js"
PROFILE="balanced"
BASE_URL=""
VUS=0
MAX_VUS=0
DURATION=""
NO_MONITOR=false

# Parse command line options
while [[ $# -gt 0 ]]; do
  case "$1" in
    -s|--scenario)
      SCENARIO="$2"
      shift 2
      ;;
    -p|--profile)
      PROFILE="$2"
      shift 2
      ;;
    -b|--base-url)
      BASE_URL="$2"
      shift 2
      ;;
    -u|--vus)
      VUS="$2"
      shift 2
      ;;
    -m|--max-vus)
      MAX_VUS="$2"
      shift 2
      ;;
    -d|--duration)
      DURATION="$2"
      shift 2
      ;;
    --no-monitor)
      NO_MONITOR=true
      shift
      ;;
    scenarios/*)
      SCENARIO="$1"
      shift
      ;;
    -*)
      echo "Unknown option: $1"
      exit 1
      ;;
    *)
      if [[ "$SCENARIO" == "scenarios/loadTest.js" ]]; then
        SCENARIO="$1"
      fi
      shift
      ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
K6_ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
RESULTS_DIR="$K6_ROOT_DIR/results"
mkdir -p "$RESULTS_DIR"

TEST_TIMESTAMP=$(date +"%Y%m%d-%H%M%S")
CSV_METRICS="$RESULTS_DIR/resources-$TEST_TIMESTAMP.csv"
SUMMARY_JSON="$RESULTS_DIR/summary-$TEST_TIMESTAMP.json"

# Automatically suppress local process monitor when targeting a remote host
if [[ -n "$BASE_URL" && ! "$BASE_URL" =~ (localhost|127\.0\.0\.1) ]]; then
  NO_MONITOR=true
fi

echo ""
echo "=========================================================="
echo "   SUPPORTDESK PERFORMANCE TEST RUNNER (BASH)             "
echo "=========================================================="
echo " Scenario  : $SCENARIO"
echo " Profile   : $PROFILE"
[[ -n "$BASE_URL" ]] && echo " BaseUrl   : $BASE_URL"
[[ $VUS -gt 0 ]]     && echo " VUs       : $VUS"
[[ $MAX_VUS -gt 0 ]] && echo " Max VUs   : $MAX_VUS"
if [[ "$SCENARIO" =~ loadTest ]]; then
  [[ -n "$DURATION" ]] && echo " Steady Dur: $DURATION (Ramp-up: 1m, Ramp-down: 30s)" || echo " Steady Dur: 8m (Ramp-up: 1m, Ramp-down: 30s)"
else
  [[ -n "$DURATION" ]] && echo " Duration  : $DURATION"
fi
echo -e "==========================================================\n"

MONITOR_PID=""
cleanup() {
  if [[ -n "$MONITOR_PID" ]]; then
    echo -e "\n[*] Stopping background resource monitor..."
    kill -TERM "$MONITOR_PID" 2>/dev/null || true
    wait "$MONITOR_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

if [[ "$NO_MONITOR" == false ]]; then
  echo "[*] Launching background resource monitor (CPU / RAM)..."
  "$SCRIPT_DIR/monitor-resources.sh" 2 "$CSV_METRICS" &
  MONITOR_PID=$!
  sleep 1
fi

K6_ARGS=("run")
[[ -n "$PROFILE" ]]  && K6_ARGS+=("-e" "PROFILE=$PROFILE")
[[ -n "$BASE_URL" ]] && K6_ARGS+=("-e" "BASE_URL=$BASE_URL")

if [[ "$SCENARIO" =~ loadTest ]]; then
  [[ $VUS -gt 0 ]]     && K6_ARGS+=("-e" "VUS=$VUS")
  [[ $MAX_VUS -gt 0 ]] && K6_ARGS+=("-e" "MAX_VUS=$MAX_VUS")
  [[ -n "$DURATION" ]] && K6_ARGS+=("-e" "DURATION=$DURATION" "-e" "STEADY_DURATION=$DURATION")
  K6_ARGS+=("-e" "SUMMARY_PATH=$SUMMARY_JSON")
else
  [[ $VUS -gt 0 ]]     && K6_ARGS+=("--vus" "$VUS")
  [[ $MAX_VUS -gt 0 ]] && K6_ARGS+=("-e" "MAX_VUS=$MAX_VUS")
  [[ -n "$DURATION" ]] && K6_ARGS+=("--duration" "$DURATION")
  K6_ARGS+=("--summary-export" "$SUMMARY_JSON")
fi

K6_ARGS+=("$SCENARIO")

echo -e "[*] Starting k6 execution...\n"
k6 "${K6_ARGS[@]}"
