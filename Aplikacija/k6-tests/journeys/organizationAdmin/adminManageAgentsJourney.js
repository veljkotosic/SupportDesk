import { group } from 'k6';
import { organizationDashboardHubUrl } from '../../config/environment.js';
import { OrganizationService } from '../../services/organizationService.js';
import { SupportAgentInviteService } from '../../services/supportAgentInviteService.js';
import { withSignalRHub } from '../../utils/signalR.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';

/**
 * Organization Admin Manage Agents Journey (mapped from OrganizationAdminAgentsView.vue):
 * 1. Log in.
 * 2. Connect to /hubs/organizationDashboardHub.
 * 3. Fetch support agents summary.
 * 4. Generate an invite code for a new agent.
 * 5. Conclude session.
 */
export function adminManageAgentsJourney(adminCredentials) {
    startSession(adminCredentials, 'Admin');

    withSignalRHub(organizationDashboardHubUrl, () => {
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
    });

    group('Admin - Session End', () => {
        finishSession();
    });
}
