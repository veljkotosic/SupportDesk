export const DEFAULT_PASSWORD = 'Password123!';

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

export function getCustomerCredentials(index = 1) {
    const idx = Math.max(1, ((index - 1) % 10000) + 1);
    return {
        email: `customer${idx}@test.com`,
        password: DEFAULT_PASSWORD,
    };
}

export function getOrgAdminCredentials(index = 1) {
    const idx = Math.max(1, ((index - 1) % 273) + 1);
    return {
        email: `admin${idx}@org${idx}.com`,
        password: DEFAULT_PASSWORD,
    };
}

export function getAgentCredentials(index = 1) {
    const idx = Math.max(0, (index - 1) % SEEDED_AGENT_EMAILS.length);
    return {
        email: SEEDED_AGENT_EMAILS[idx],
        password: DEFAULT_PASSWORD,
    };
}

/**
 * Returns appropriate credentials based on the journey role.
 * Spreads VUs across distinct entity slices to eliminate account contention.
 * @param {'Customer' | 'Agent' | 'Org Admin' | 'Admin'} role 
 * @param {number} [vuId=1] 
 * @param {number} [iter=0] 
 */
export function getCredentialsForRole(role, vuId = 1, iter = 0) {
    const safeVu = vuId || 1;
    const safeIter = iter || 0;
    const offset = ((safeVu - 1) * 7 + safeIter) + 1;
    switch (role) {
        case 'Customer':
            return getCustomerCredentials(offset);
        case 'Agent':
            return getAgentCredentials(offset);
        case 'Org Admin':
        case 'Admin':
            return getOrgAdminCredentials(offset);
        default:
            return getCustomerCredentials(offset);
    }
}

