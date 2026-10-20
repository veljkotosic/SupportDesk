import { baseOptions } from '../config/options.js';
import { executeWorkloadIteration } from './workloadRunner.js';

/**
 * Scenario: Read-Heavy Workload (90% Read / 10% Write)
 * Simulates typical daytime browsing, reporting, and monitoring:
 * - 90% read queries (customer & agent queues, org admin analytics dashboard)
 * - 10% write mutations (occasional tickets and notes)
 * 
 * Execution:
 *   k6 run scenarios/readHeavy.js
 *   k6 run --vus 20 --duration 2m scenarios/readHeavy.js
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
    executeWorkloadIteration('readHeavy');
}
