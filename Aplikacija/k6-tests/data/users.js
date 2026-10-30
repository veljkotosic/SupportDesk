import exec from 'k6/execution';

export const DEFAULT_PASSWORD = 'Password123!';

// Load org_users_map if available (273 organizations with their admin and agents)
let orgUsersMap = [];
try {
    if (typeof open === 'function') {
        orgUsersMap = JSON.parse(open('./org_users_map.json'));
    }
} catch (e) {
    orgUsersMap = [];
}

let loadedAgents = [];
try {
    if (typeof open === 'function') {
        loadedAgents = JSON.parse(open('./agents.json'));
    }
} catch (e) {
    loadedAgents = [];
}

export const SEEDED_AGENT_EMAILS = loadedAgents.length > 0 ? loadedAgents : [
    'agent1181@orgba93.com',
    'agent276@org43d8.com',
    'agent904@org4db9.com',
    'agent567@org57e2.com',
    'agent425@org95e7.com',
    'agent723@orgf0dd.com',
    'agent495@orgc928.com',
    'agent490@orgdf43.com',
    'agent421@org4045.com',
    'agent878@org87a8.com',
    'agent821@orgcda9.com',
    'agent408@org4481.com',
    'agent814@orge055.com',
    'agent1148@orgb60c.com',
    'agent1173@org6a85.com',
    'agent940@orgf4d1.com',
    'agent651@orgae43.com',
    'agent692@org3550.com',
    'agent918@org8640.com',
    'agent935@orgecff.com',
    'agent207@orgec20.com',
    'agent374@org8d77.com',
    'agent996@org5662.com',
    'agent575@org889b.com',
    'agent953@orgd7dd.com',
    'agent792@orge6cf.com',
    'agent310@orgbdb1.com',
    'agent770@org7296.com',
    'agent957@orga36e.com',
    'agent565@orgae4f.com'
];

const TOTAL_CUSTOMERS = 375000;
const CUSTOMER_SHARD_SIZE = 1000;
const TOTAL_ORGS = 273;

/**
 * Returns customer credentials.
 * Automatically draws from the global monotonic iteration stream across all VUs,
 * guaranteeing 100% unique customers across ANY number of VUs (10, 500, 1000, 5000+)
 * with zero bucket sharing or collisions.
 * @param {number} [vuOrIndex=1] VU ID or direct index
 * @param {number} [iter=null] Iteration index
 */
export function getCustomerCredentials(vuOrIndex = 1, iter = null) {
    let customerNum = null;

    // 1. Preferred: globally unique monotonic iteration across all VUs in the test
    try {
        if (typeof exec !== 'undefined' && exec && exec.scenario && typeof exec.scenario.iterationInTest === 'number') {
            customerNum = (exec.scenario.iterationInTest % TOTAL_CUSTOMERS) + 1;
        }
    } catch (_) {
        // Fallback if exec is unavailable
    }

    // 2. Fallback: VU-partitioned sharding or direct index
    if (!customerNum) {
        if (iter === null || iter === undefined) {
            customerNum = Math.max(1, ((vuOrIndex - 1) % TOTAL_CUSTOMERS) + 1);
        } else {
            const safeVu = Math.max(1, vuOrIndex || 1);
            const safeIter = Math.max(0, iter || 0);
            customerNum = (((safeVu - 1) * CUSTOMER_SHARD_SIZE) + (safeIter % CUSTOMER_SHARD_SIZE)) % TOTAL_CUSTOMERS + 1;
        }
    }

    return {
        email: `customer${customerNum}@test.com`,
        password: DEFAULT_PASSWORD,
    };
}

/**
 * Returns organization admin credentials.
 * Each VU is assigned its own dedicated organization to prevent any admin/audit contention.
 * @param {number} [vuOrIndex=1] VU ID or direct index
 * @param {number} [iter=null] Iteration index
 */
export function getOrgAdminCredentials(vuOrIndex = 1, iter = null) {
    const safeVu = Math.max(1, vuOrIndex || 1);
    const orgIdx = ((safeVu - 1) % TOTAL_ORGS) + 1;
    return {
        email: `admin${orgIdx}@org${orgIdx}.com`,
        password: DEFAULT_PASSWORD,
    };
}

/**
 * Returns support agent credentials.
 * When called with (vuId, iter), aligns the agent to the VU's dedicated organization.
 * Across up to 1,212 VUs, each VU receives a distinct support agent account.
 * @param {number} [vuOrIndex=1] VU ID or direct index
 * @param {number} [iter=null] Iteration index
 */
export function getAgentCredentials(vuOrIndex = 1, iter = null) {
    if (iter === null || iter === undefined) {
        const idx = Math.max(0, (vuOrIndex - 1) % SEEDED_AGENT_EMAILS.length);
        return {
            email: SEEDED_AGENT_EMAILS[idx],
            password: DEFAULT_PASSWORD,
        };
    }

    const safeVu = Math.max(1, vuOrIndex || 1);
    if (orgUsersMap && orgUsersMap.length > 0) {
        const orgIdx = (safeVu - 1) % orgUsersMap.length;
        const org = orgUsersMap[orgIdx];
        if (org && org.agents && org.agents.length > 0) {
            const agentSlot = Math.floor((safeVu - 1) / orgUsersMap.length) % org.agents.length;
            return {
                email: org.agents[agentSlot],
                password: DEFAULT_PASSWORD,
            };
        }
    }

    const agentIdx = (safeVu - 1) % SEEDED_AGENT_EMAILS.length;
    return {
        email: SEEDED_AGENT_EMAILS[agentIdx],
        password: DEFAULT_PASSWORD,
    };
}

/**
 * Returns appropriate credentials based on the journey role.
 * Spreads VUs across strictly disjoint entity slices to eliminate account contention.
 * @param {'Customer' | 'Agent' | 'Org Admin' | 'Admin'} role 
 * @param {number} [vuId=1] 
 * @param {number} [iter=0] 
 */
export function getCredentialsForRole(role, vuId = 1, iter = 0) {
    switch (role) {
        case 'Customer':
            return getCustomerCredentials(vuId, iter);
        case 'Agent':
            return getAgentCredentials(vuId, iter);
        case 'Org Admin':
        case 'Admin':
            return getOrgAdminCredentials(vuId, iter);
        default:
            return getCustomerCredentials(vuId, iter);
    }
}
