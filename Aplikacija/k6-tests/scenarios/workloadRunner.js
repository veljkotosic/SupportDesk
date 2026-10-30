import { Counter, Trend } from 'k6/metrics';
import exec from 'k6/execution';
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
 * Assigns a stable role to a VU to maintain strict percentage breakdown:
 * - 65% Customers (13/20)
 * - 20% Support Agents (4/20)
 * - 15% Org Admins (3/20)
 * Works consistently across any number of VUs.
 * @param {number} vuId 
 * @returns {'Customer' | 'Agent' | 'Org Admin'}
 */
export function getRoleForVu(vuId) {
    const safeVu = Math.max(1, vuId || 1);
    const mod = (safeVu - 1) % 20;
    if (mod < 13) return 'Customer';
    if (mod < 17) return 'Agent';
    return 'Org Admin';
}

/**
 * Executes a single weighted iteration for the given workload profile.
 * Automatically resolves credentials and records operational metrics.
 * 
 * @param {string | { readRatio: number, writeRatio: number }} profileKeyOrConfig 
 * @param {boolean} [isSteadyState=true] Whether this iteration belongs to the steady-state test phase
 */
export function executeWorkloadIteration(profileKeyOrConfig, isSteadyState = true) {
    const vuId = (typeof exec !== 'undefined' && exec && exec.vu && exec.vu.idInScenario) ? exec.vu.idInScenario : (typeof __VU !== 'undefined' ? __VU : 1);
    const iter = (typeof exec !== 'undefined' && exec && exec.vu && exec.vu.iterationInScenario) ? exec.vu.iterationInScenario : (typeof __ITER !== 'undefined' ? __ITER : 0);

    const role = getRoleForVu(vuId);
    const calibratedJourneys = getCalibratedJourneys(profileKeyOrConfig, role);
    const selected = selectWeightedJourney(calibratedJourneys);

    if (isSteadyState) {
        if (selected.category === 'read') {
            readOperationsTotal.add(1, { journey: selected.id });
        } else {
            writeOperationsTotal.add(1, { journey: selected.id });
        }
    }

    const start = Date.now();
    try {
        if (selected.requiresCredentials) {
            const credentials = getCredentialsForRole(role, vuId, iter);
            selected.fn(credentials);
        } else {
            selected.fn();
        }
    } finally {
        if (isSteadyState) {
            const duration = Date.now() - start;
            journeyExecutionDuration.add(duration, {
                journey: selected.id,
                category: selected.category,
                role: selected.role,
            });
        }
    }
}
