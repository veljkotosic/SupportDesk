import { executeWorkloadIteration } from './workloadRunner.js';
import { standardThresholds } from '../config/thresholds.js';
import { standardSummaryTrendStats } from '../config/summaryTrendStats.js';

/**
 * Standard Production Load Test (10 - 15 Minute Steady-State Run)
 * 
 * Verifies system stability, throughput, and latency under sustained target concurrency:
 * - 2 minutes: Smooth ramp-up to target concurrency (heats caches, connection pools).
 * - 8 minutes: Steady-state load (monitors GC cycles, connection reuse, memory stability).
 * - 2 minutes: Smooth ramp-down.
 * 
 * Target VUs should typically be set to 70% - 80% of the peak discovered via the Breakpoint Test.
 * 
 * Execution:
 *   k6 run scenarios/loadTest.js
 *   k6 run -e VUS=50 scenarios/loadTest.js
 *   k6 run -e VUS=50 -e PROFILE=readHeavy scenarios/loadTest.js
 */
const targetVUs = Number(__ENV.MAX_VUS || __ENV.VUS || 30);
const rampUpDuration = __ENV.RAMP_UP || '2m';
const steadyDuration = __ENV.STEADY_DURATION || '8m';
const rampDownDuration = __ENV.RAMP_DOWN || '2m';

export const options = {
    summaryTrendStats: standardSummaryTrendStats,
    stages: [
        { duration: rampUpDuration, target: targetVUs },   // Ramp-up
        { duration: steadyDuration, target: targetVUs },   // Steady-state
        { duration: rampDownDuration, target: 0 },         // Ramp-down
    ],
    thresholds: standardThresholds,
};

const profileKey = __ENV.PROFILE || 'balanced';

export default function () {
    executeWorkloadIteration(profileKey);
}
