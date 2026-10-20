import { baseOptions } from '../config/options.js';
import { executeWorkloadIteration } from './workloadRunner.js';

/**
 * Scenario: Write-Heavy Workload (10% Read / 90% Write)
 * Extreme write stress test testing database write throughput, locks,
 * transaction commit speed, RabbitMQ outbox dispatchers, and SignalR broadcasts:
 * - 10% read operations (minimum necessary data verification)
 * - 90% write mutations (ticket creation, chats, registrations, notes, resolves)
 * 
 * Execution:
 *   k6 run scenarios/writeHeavy.js
 *   k6 run --vus 20 --duration 2m scenarios/writeHeavy.js
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
    executeWorkloadIteration('writeHeavy');
}
