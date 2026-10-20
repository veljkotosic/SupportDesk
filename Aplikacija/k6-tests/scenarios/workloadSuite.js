import { baseOptions } from '../config/options.js';
import { executeWorkloadIteration } from './workloadRunner.js';
import { WORKLOAD_PROFILES } from '../config/workloads.js';

/**
 * Universal Workload Suite Runner
 * 
 * Supports executing any pre-defined profile OR custom dynamic ratios via environment variables:
 * 
 * Examples:
 *   k6 run -e PROFILE=readHeavy scenarios/workloadSuite.js
 *   k6 run -e PROFILE=readDominant_75_25 scenarios/workloadSuite.js
 *   k6 run -e PROFILE=balanced scenarios/workloadSuite.js
 *   k6 run -e PROFILE=writeDominant_25_75 scenarios/workloadSuite.js
 *   k6 run -e PROFILE=writeHeavy scenarios/workloadSuite.js
 * 
 * Dynamic Custom Ratio:
 *   k6 run -e READ_RATIO=0.8 -e WRITE_RATIO=0.2 scenarios/workloadSuite.js
 */
export const options = {
    ...baseOptions,
    stages: [
        { duration: __ENV.RAMP_UP || '30s', target: Number(__ENV.VUS || 10) },
        { duration: __ENV.DURATION || '1m', target: Number(__ENV.VUS || 10) },
        { duration: __ENV.RAMP_DOWN || '20s', target: 0 },
    ],
};

const profileKey = __ENV.PROFILE || 'balanced';

const customConfig = __ENV.READ_RATIO && __ENV.WRITE_RATIO ? {
    readRatio: Number(__ENV.READ_RATIO),
    writeRatio: Number(__ENV.WRITE_RATIO),
} : null;

export default function () {
    executeWorkloadIteration(customConfig || profileKey);
}
