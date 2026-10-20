import { baseOptions } from '../config/options.js';
import { executeWorkloadIteration } from './workloadRunner.js';

/**
 * Scenario: Write-Dominant Workload (25% Read / 75% Write)
 * Simulates intense transactional spikes: mass ticket submissions, high-frequency
 * chat exchanges, ticket claims, resolutions, and account registrations:
 * - 25% read operations (occasional queue checks and dashboard views)
 * - 75% write operations (mutations, SignalR broadcasts, RabbitMQ events)
 * 
 * Execution:
 *   k6 run scenarios/writeDominant_25_75.js
 *   k6 run --vus 20 --duration 2m scenarios/writeDominant_25_75.js
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
    executeWorkloadIteration('writeDominant_25_75');
}
