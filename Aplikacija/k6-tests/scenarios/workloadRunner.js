import { Counter, Trend } from 'k6/metrics';
import { getCalibratedJourneys, WORKLOAD_PROFILES } from '../config/workloads.js';
import { getCredentialsForRole } from '../data/users.js';

export const readOperationsTotal = new Counter('workload_read_journeys_total');
export const writeOperationsTotal = new Counter('workload_write_journeys_total');
export const journeyExecutionDuration = new Trend('workload_journey_duration_ms', true);

/**
 * Picks a journey using the calibrated cumulative distribution.
 * @param {Array<Object>} calibratedJourneys 
 * @returns {Object} Selected journey descriptor
 */
export function selectWeightedJourney(calibratedJourneys) {
    const random = Math.random();
    for (const journey of calibratedJourneys) {
        if (random <= journey.cumulativeWeight) {
            return journey;
        }
    }

    return calibratedJourneys[calibratedJourneys.length - 1];
}

/**
 * Executes a single weighted iteration for the given workload profile.
 * Automatically resolves credentials and records operational metrics.
 * 
 * @param {string | { readRatio: number, writeRatio: number }} profileKeyOrConfig 
 */
export function executeWorkloadIteration(profileKeyOrConfig) {
    const calibratedJourneys = getCalibratedJourneys(profileKeyOrConfig);
    const selected = selectWeightedJourney(calibratedJourneys);

    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;

    if (selected.category === 'read') {
        readOperationsTotal.add(1, { journey: selected.id });
    } else {
        writeOperationsTotal.add(1, { journey: selected.id });
    }

    const start = Date.now();
    try {
        if (selected.requiresCredentials) {
            const credentials = getCredentialsForRole(selected.role, vuId, iter);
            selected.fn(credentials);
        } else {
            selected.fn();
        }
    } finally {
        const duration = Date.now() - start;
        journeyExecutionDuration.add(duration, {
            journey: selected.id,
            category: selected.category,
            role: selected.role,
        });
    }
}
