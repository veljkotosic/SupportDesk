import { baseOptions } from '../config/options.js';
import { executeWorkloadIteration } from './workloadRunner.js';

/**
 * Scenario: Read-Dominant Workload (75% Read / 25% Write)
 * Simulates high browsing and queue monitoring accompanied by regular
 * customer ticket submissions, chats, and claims:
 * - 75% read operations (dashboards, queues, filters)
 * - 25% write operations (tickets, chat replies, internal notes)
 * 
 * Execution:
 *   k6 run scenarios/readDominant_75_25.js
 *   k6 run --vus 20 --duration 2m scenarios/readDominant_75_25.js
 */
export const options = {
    ...baseOptions,
    stages: [
        { duration: __ENV.RAMP_UP || '30s', target: Number(__ENV.VUS || 10) },
        { duration: __ENV.DURATION || '1m', target: Number(__ENV.VUS || 10) },
        { duration: __ENV.RAMP_DOWN || '20s', target: 0 },
    ],
};

export default function () {
    executeWorkloadIteration('readDominant_75_25');
}
