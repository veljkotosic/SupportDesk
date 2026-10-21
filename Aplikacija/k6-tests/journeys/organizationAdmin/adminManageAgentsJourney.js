import { group } from 'k6';
import { OrganizationService } from '../../services/organizationService.js';
import { SupportAgentInviteService } from '../../services/supportAgentInviteService.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';

/**
 * Organization Admin Manage Agents Journey (mapped from OrganizationAdminAgentsView.vue):
 * 1. Log in.
 * 2. Fetch support agents summary.
 * 3. Generate an invite code for a new agent.
 * 4. Conclude session.
 */
export function adminManageAgentsJourney(adminCredentials) {
    startSession(adminCredentials, 'Admin');

    group('Admin - View Support Agents Summary', () => {
        OrganizationService.getSupportAgentsSummary();
    });

    group('Admin - Invite New Agent', () => {
        waitBetween(2, 4);
        const vuId = typeof __VU !== 'undefined' ? __VU : 1;
        const iter = typeof __ITER !== 'undefined' ? __ITER : 0;
        const rand = Math.random().toString(36).substring(2, 8);
        const randomEmail = `agent_inv_${vuId}_${iter}_${Date.now()}_${rand}@loadtest.com`;
        SupportAgentInviteService.createSupportAgentInvite(randomEmail);
    });

    group('Admin - Session End', () => {
        finishSession();
    });
}
