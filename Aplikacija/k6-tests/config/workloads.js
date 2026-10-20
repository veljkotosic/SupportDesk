import * as journeys from '../journeys/index.js';

export const JOURNEY_METADATA = [
    {
        id: 'customer_browse_dashboard',
        name: 'Customer - Browse Dashboard',
        category: 'read',
        role: 'Customer',
        subWeight: 0.35,
        requiresCredentials: true,
        fn: journeys.customerBrowseDashboardJourney,
    },
    {
        id: 'agent_browse_tickets',
        name: 'Agent - Browse Ticket Queue',
        category: 'read',
        role: 'Agent',
        subWeight: 0.35,
        requiresCredentials: true,
        fn: journeys.agentBrowseTicketsJourney,
    },
    {
        id: 'org_admin_dashboard',
        name: 'Org Admin - Analytics Dashboard',
        category: 'read',
        role: 'Org Admin',
        subWeight: 0.15,
        requiresCredentials: true,
        fn: journeys.orgAdminDashboardJourney,
    },
    {
        id: 'admin_browse_tickets',
        name: 'Org Admin - Browse Tickets Queue',
        category: 'read',
        role: 'Org Admin',
        subWeight: 0.15,
        requiresCredentials: true,
        fn: journeys.adminBrowseTicketsJourney,
    },

    {
        id: 'customer_open_ticket',
        name: 'Customer - Open Ticket',
        category: 'write',
        role: 'Customer',
        subWeight: 0.20,
        requiresCredentials: true,
        fn: journeys.customerOpenTicketJourney,
    },
    {
        id: 'customer_chat',
        name: 'Customer - Ticket Chat',
        category: 'write',
        role: 'Customer',
        subWeight: 0.20,
        requiresCredentials: true,
        fn: journeys.customerChatJourney,
    },
    {
        id: 'agent_chat',
        name: 'Agent - Ticket Chat',
        category: 'write',
        role: 'Agent',
        subWeight: 0.15,
        requiresCredentials: true,
        fn: journeys.agentChatJourney,
    },
    {
        id: 'agent_resolve_ticket',
        name: 'Agent - Resolve & Close Ticket',
        category: 'write',
        role: 'Agent',
        subWeight: 0.15,
        requiresCredentials: true,
        fn: journeys.agentResolveTicketJourney,
    },
    {
        id: 'agent_claim_ticket',
        name: 'Agent - Claim Open Ticket',
        category: 'write',
        role: 'Agent',
        subWeight: 0.10,
        requiresCredentials: true,
        fn: journeys.agentClaimTicketJourney,
    },
    {
        id: 'customer_feedback',
        name: 'Customer - Rate Closed Ticket',
        category: 'write',
        role: 'Customer',
        subWeight: 0.05,
        requiresCredentials: true,
        fn: journeys.customerFeedbackJourney,
    },
    {
        id: 'customer_registration',
        name: 'Customer - Self Registration',
        category: 'write',
        role: 'Customer',
        subWeight: 0.05,
        requiresCredentials: false,
        fn: journeys.customerRegistrationJourney,
    },
    {
        id: 'admin_knowledge_base',
        name: 'Org Admin - Manage Knowledge Base',
        category: 'write',
        role: 'Org Admin',
        subWeight: 0.04,
        requiresCredentials: true,
        fn: journeys.adminKnowledgeBaseJourney,
    },
    {
        id: 'admin_view_ticket_note',
        name: 'Org Admin - Audit Ticket & Add Note',
        category: 'write',
        role: 'Org Admin',
        subWeight: 0.03,
        requiresCredentials: true,
        fn: journeys.adminViewTicketJourney,
    },
    {
        id: 'admin_manage_agents',
        name: 'Org Admin - Invite Agent',
        category: 'write',
        role: 'Org Admin',
        subWeight: 0.02,
        requiresCredentials: true,
        fn: journeys.adminManageAgentsJourney,
    },
    {
        id: 'org_registration',
        name: 'Org Admin - Register New Org',
        category: 'write',
        role: 'Org Admin',
        subWeight: 0.01,
        requiresCredentials: false,
        fn: journeys.orgRegistrationJourney,
    },
];
export const WORKLOAD_PROFILES = {
    readHeavy: {
        id: 'read_heavy',
        name: 'Read Heavy',
        readRatio: 0.90,
        writeRatio: 0.10,
        description: 'Simulates typical read-dominated traffic: dashboard inspection, monitoring queues, browsing tickets.',
    },

    readDominant_75_25: {
        id: 'read_dominant_75_25',
        name: 'Read Dominant (75/25)',
        readRatio: 0.75,
        writeRatio: 0.25,
        description: 'Moderate read-bias traffic: high browsing activity accompanied by regular incoming tickets and active chats.',
    },

    balanced: {
        id: 'balanced',
        name: 'Balanced (50/50)',
        readRatio: 0.50,
        writeRatio: 0.50,
        description: 'Even distribution: equal proportion of read queries and write mutations.',
    },

    writeDominant_25_75: {
        id: 'write_dominant_25_75',
        name: 'Write Dominant (25/75)',
        readRatio: 0.25,
        writeRatio: 0.75,
        description: 'Heavy write workload: surges in ticket creation, rapid back-and-forth chat exchanges, claims, and resolutions.',
    },

    writeHeavy: {
        id: 'write_heavy',
        name: 'Write Heavy',
        readRatio: 0.10,
        writeRatio: 0.90,
        description: 'Extreme mutation stress testing: system under continuous write load, database inserts, updates, and SignalR broadcasts.',
    },
};

/**
 * Computes calibrated journey weights for a given read/write ratio or profile name.
 * @param {string | { readRatio: number, writeRatio: number }} profileOrRatio 
 * @returns {Array<Object>} List of journeys with calibrated absolute weights and cumulative thresholds
 */
export function getCalibratedJourneys(profileOrRatio) {
    let profile = null;

    if (typeof profileOrRatio === 'string') {
        profile = WORKLOAD_PROFILES[profileOrRatio];
        
        if (!profile) {
            throw new Error(`Unknown workload profile: "${profileOrRatio}". Valid profiles: ${Object.keys(WORKLOAD_PROFILES).join(', ')}`);
        }
    } else if (profileOrRatio && typeof profileOrRatio === 'object') {
        profile = profileOrRatio;
    } else {
        profile = WORKLOAD_PROFILES.balanced;
    }

    const readRatio = profile.readRatio ?? 0.5;
    const writeRatio = profile.writeRatio ?? 0.5;

    let cumulative = 0;
    return JOURNEY_METADATA.map((journeys) => {
        const ratio = journeys.category === 'read' ? readRatio : writeRatio;
        const weight = ratio * journeys.subWeight;
        
        cumulative += weight;

        return {
            ...journeys,
            weight: Number(weight.toFixed(6)),
            cumulativeWeight: Number(cumulative.toFixed(6)),
        };
    });
}
