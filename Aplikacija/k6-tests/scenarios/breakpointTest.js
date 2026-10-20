import { executeWorkloadIteration } from './workloadRunner.js';
import { standardSummaryTrendStats } from '../config/summaryTrendStats.js';

/**
 * Breakpoint Test (Capacity Limit Finder)
 * 
 * Continuously ramps up virtual users (VUs) step-by-step until the system reaches its breaking point.
 * With `abortOnFail: true`, k6 immediately halts execution the moment response times exceed
 * the SLA ceiling (p95 > 1000ms) or error rates exceed 1%.
 * 
 * The number of VUs active at the moment of abortion is your system's maximum capacity!
 * 
 * Execution:
 *   k6 run scenarios/breakpointTest.js
 *   k6 run -e PROFILE=readHeavy scenarios/breakpointTest.js
 */
const maxVUs = Number(__ENV.MAX_VUS || 300);

export const options = {
    summaryTrendStats: standardSummaryTrendStats,
    stages: [
        { duration: '30s', target: Math.round(maxVUs * 0.05) },   // Warmup (5% of max)
        { duration: '1m', target: Math.round(maxVUs * 0.10) },    // 10%
        { duration: '1m', target: Math.round(maxVUs * 0.20) },    // 20%
        { duration: '1m', target: Math.round(maxVUs * 0.35) },    // 35%
        { duration: '1m', target: Math.round(maxVUs * 0.50) },    // 50%
        { duration: '1m', target: Math.round(maxVUs * 0.65) },    // 65%
        { duration: '1m', target: Math.round(maxVUs * 0.80) },    // 80%
        { duration: '1m', target: maxVUs },                      // 100% of Max VUs
    ],
    thresholds: {
        // Abort test if error rate exceeds 5% after the first 1m of warmup
        http_req_failed: [
            {
                threshold: 'rate<0.05',
                abortOnFail: true,
                delayAbortEval: '1m',
            },
        ],
        // Abort test if 95th percentile latency crosses 1.5s after 1m of warmup
        http_req_duration: [
            {
                threshold: 'p(95)<1500',
                abortOnFail: true,
                delayAbortEval: '1m',
            },
        ],
    },
};

const profileKey = __ENV.PROFILE || 'balanced';

export default function () {
    executeWorkloadIteration(profileKey);
}
