import { executeWorkloadIteration } from './workloadRunner.js';
import { standardSummaryTrendStats } from '../config/summaryTrendStats.js';
import { textSummary } from '../utils/k6-summary.js';

/**
 * Standard Production Load Test (Steady-State Run with Isolated Rampings)
 * 
 * Verifies system stability, throughput, and latency under sustained target concurrency:
 * - 1 minute: Mandatory ramp-up to target concurrency (heats caches, connection pools).
 * - Configurable steady-state load (default 8 minutes): Steady-state load (monitors GC cycles, connection reuse, memory stability).
 * - 30 seconds: Mandatory ramp-down.
 * 
 * Crucial: Ramp-up and ramp-down phases are strictly excluded from reported summary statistics
 * and exported JSON, ensuring only pure steady-state performance is captured.
 * 
 * Execution:
 *   k6 run scenarios/loadTest.js
 *   k6 run -e VUS=50 scenarios/loadTest.js
 *   k6 run -e VUS=50 -e PROFILE=readHeavy scenarios/loadTest.js
 */

function parseDurationSeconds(dur) {
    if (!dur) return 480; // default 8m
    if (typeof dur === 'number') return dur;
    const str = String(dur).trim().toLowerCase();
    if (!isNaN(str)) return Number(str);

    let totalSeconds = 0;
    const regex = /(\d+)\s*(h|m|s)?/g;
    let match;
    let matchedAny = false;
    while ((match = regex.exec(str)) !== null) {
        if (!match[1]) continue;
        matchedAny = true;
        const val = parseInt(match[1], 10);
        const unit = match[2];
        if (unit === 'h') totalSeconds += val * 3600;
        else if (unit === 'm') totalSeconds += val * 60;
        else totalSeconds += val;
    }
    return matchedAny ? totalSeconds : 480;
}

const targetVUs = Number(__ENV.MAX_VUS || __ENV.VUS || 30);
const steadyDurationRaw = __ENV.STEADY_DURATION || __ENV.DURATION || '8m';
const steadyDurationSec = parseDurationSeconds(steadyDurationRaw);
const rampDownStartTimeSec = 60 + steadyDurationSec;

export const options = {
    summaryTrendStats: standardSummaryTrendStats,
    scenarios: {
        ramp_up: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [{ duration: '1m', target: targetVUs }],
            exec: 'rampUpIteration',
            gracefulRampDown: '30s',
            gracefulStop: '30s',
        },
        steady_state: {
            executor: 'constant-vus',
            vus: targetVUs,
            duration: `${steadyDurationSec}s`,
            startTime: '1m',
            exec: 'steadyStateIteration',
            gracefulStop: '30s',
        },
        ramp_down: {
            executor: 'ramping-vus',
            startVUs: targetVUs,
            stages: [{ duration: '30s', target: 0 }],
            startTime: `${rampDownStartTimeSec}s`,
            exec: 'rampDownIteration',
            gracefulRampDown: '30s',
            gracefulStop: '30s',
        },
    },
    thresholds: {
        'http_req_failed{scenario:steady_state}': ['rate<0.01'],
        'http_req_duration{scenario:steady_state}': [
            'p(95)<500',
            'p(99)<1000',
        ],
        'http_reqs{scenario:steady_state}': [],
        'iterations{scenario:steady_state}': [],
        'iteration_duration{scenario:steady_state}': [],
        'checks{scenario:steady_state}': [],
        'data_received{scenario:steady_state}': [],
        'data_sent{scenario:steady_state}': [],
        'http_req_blocked{scenario:steady_state}': [],
        'http_req_connecting{scenario:steady_state}': [],
        'http_req_tls_handshaking{scenario:steady_state}': [],
        'http_req_sending{scenario:steady_state}': [],
        'http_req_waiting{scenario:steady_state}': [],
        'http_req_receiving{scenario:steady_state}': [],
    },
};

const profileKey = __ENV.PROFILE || 'balanced';

export function rampUpIteration() {
    executeWorkloadIteration(profileKey, false);
}

export function steadyStateIteration() {
    executeWorkloadIteration(profileKey, true);
}

export function rampDownIteration() {
    executeWorkloadIteration(profileKey, false);
}

export default function () {
    executeWorkloadIteration(profileKey, true);
}

export function handleSummary(data) {
    const cleanData = JSON.parse(JSON.stringify(data));

    // Overwrite base metrics with steady_state tagged metrics
    for (const [key, val] of Object.entries(cleanData.metrics)) {
        const match = key.match(/^(.+)\{scenario:steady_state\}$/);
        if (match) {
            const baseMetric = match[1];
            cleanData.metrics[baseMetric] = val;
            if (val.values && typeof val.values.count === 'number' && val.values.rate !== undefined) {
                val.values.rate = val.values.count / steadyDurationSec;
            }
        }
    }

    // Ensure counter rates reflect steady-state duration
    for (const [key, val] of Object.entries(cleanData.metrics)) {
        if (val.type === 'counter' && val.values && typeof val.values.count === 'number' && typeof val.values.rate === 'number') {
            val.values.rate = val.values.count / steadyDurationSec;
        }
    }

    // Set VUs to targetVUs for steady-state reporting
    if (cleanData.metrics.vus) {
        cleanData.metrics.vus.values.value = targetVUs;
        cleanData.metrics.vus.values.min = targetVUs;
        cleanData.metrics.vus.values.max = targetVUs;
    }
    if (cleanData.metrics.vus_max) {
        cleanData.metrics.vus_max.values.value = targetVUs;
        cleanData.metrics.vus_max.values.min = targetVUs;
        cleanData.metrics.vus_max.values.max = targetVUs;
    }

    // Strip all {scenario:...} tagged keys from display metrics
    for (const key of Object.keys(cleanData.metrics)) {
        if (key.includes('{scenario:')) {
            delete cleanData.metrics[key];
        }
    }

    const summaryExportPath = __ENV.SUMMARY_PATH || __ENV.SUMMARY_EXPORT || '';
    const result = {
        'stdout': '\n' + textSummary(cleanData, { indent: ' ', enableColors: true }),
    };

    if (summaryExportPath) {
        result[summaryExportPath] = JSON.stringify(cleanData, null, 2);
    }

    return result;
}
