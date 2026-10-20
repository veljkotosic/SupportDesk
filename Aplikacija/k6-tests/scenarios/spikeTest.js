import { executeWorkloadIteration } from './workloadRunner.js';
import { standardSummaryTrendStats } from '../config/summaryTrendStats.js';

/**
 * Spike Test (Traffic Surge & Recovery Test)
 * 
 * Simulates a sudden, massive influx of traffic (e.g. mass notification or incident alert),
 * followed by a rapid drop back to normal baseline, to verify:
 * 1. Does the application absorb the shock without crashing or exhausting thread/connection pools?
 * 2. Does the system recover quickly once the spike subsides?
 * 3. Does RabbitMQ drain queued messages and SignalR recover normal frame latency?
 * 
 * Traffic Shape:
 *   [ 00:00 - 00:30 ] Baseline (5 VUs)
 *   [ 00:30 - 00:40 ] SUDDEN SPIKE! (5 -> 80 VUs in 10s)
 *   [ 00:40 - 01:40 ] Hold Spike Peak (80 VUs for 1m)
 *   [ 01:40 - 01:50 ] Sharp Drop (80 -> 5 VUs in 10s)
 *   [ 01:50 - 03:00 ] Recovery Period (5 VUs for 1m10s)
 * 
 * Execution:
 *   k6 run scenarios/spikeTest.js
 *   k6 run -e SPIKE_VUS=120 scenarios/spikeTest.js
 */
const spikeVUs = Number(__ENV.MAX_VUS || __ENV.SPIKE_VUS || 80);
const baselineVUs = Number(__ENV.BASELINE_VUS || 5);

export const options = {
    summaryTrendStats: standardSummaryTrendStats,
    stages: [
        { duration: '30s', target: baselineVUs }, // Baseline
        { duration: '10s', target: spikeVUs },    // Fast spike up
        { duration: '1m', target: spikeVUs },     // Hold spike peak
        { duration: '10s', target: baselineVUs }, // Fast drop down
        { duration: '1m10s', target: baselineVUs }, // Recovery observation
    ],
    thresholds: {
        // Tolerates brief error bursts during the spike peak, but ensures overall system integrity
        http_req_failed: ['rate<0.05'],
        // 95th percentile during the combined spike should stay below 1.5s
        http_req_duration: ['p(95)<1500', 'p(99)<2500'],
    },
};

const profileKey = __ENV.PROFILE || 'balanced';

export default function () {
    executeWorkloadIteration(profileKey);
}
