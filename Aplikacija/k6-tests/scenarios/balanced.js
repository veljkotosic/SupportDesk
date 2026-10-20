import { baseOptions } from '../config/options.js';
import { executeWorkloadIteration } from './workloadRunner.js';

/**
 * Scenario: Balanced Workload (50% Read / 50% Write)
 * Represents typical peak operational hours where incoming ticket submissions,
 * interactive customer/agent messaging, claims, and resolutions equal the volume
 * of dashboard reviews and ticket queue searches:
 * - 50% read operations
 * - 50% write operations
 * 
 * Execution:
 *   k6 run scenarios/balanced.js
 *   k6 run --vus 20 --duration 2m scenarios/balanced.js
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
    executeWorkloadIteration('balanced');
}
