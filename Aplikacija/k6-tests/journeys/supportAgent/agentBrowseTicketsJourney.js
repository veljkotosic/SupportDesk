import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { withSignalRHub } from '../../utils/signalR.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { TicketStatus, TicketPriority, TicketSortOption } from '../../utils/enums.js';
import { organizationDashboardHubUrl } from '../../config/environment.js';

/**
 * Support Agent Browse Tickets Journey (mapped from OrganizationTicketList.vue):
 * 1. Log in.
 * 2. Connect to organizationDashboardHubUrl.
 * 3. Load default queue (page 1, Latest).
 * 4. Think time, then switch filters:
 *    - Filter by Open tickets.
 *    - Filter by High Priority sorted by Priority.
 * 5. Conclude session.
 */
export function agentBrowseTicketsJourney(agentCredentials) {
    startSession(agentCredentials, 'Agent');

    withSignalRHub(organizationDashboardHubUrl, (socket, stats) => {
        group('Agent - Load Default Ticket Queue', () => {
            TicketService.getOrganizationTickets(0, 10, '', null, null, TicketSortOption.Latest);
        });

        group('Agent - Filter by Open Tickets', () => {
            waitBetween(2, 4);
            TicketService.getOrganizationTickets(0, 10, '', TicketStatus.Open, null, TicketSortOption.Latest);
        });

        group('Agent - Filter High Priority', () => {
            waitBetween(1, 3);
            TicketService.getOrganizationTickets(0, 10, '', null, TicketPriority.High, TicketSortOption.Priority);
        });

        if (stats.incomingCount > 0) {
            waitBetween(1, 2);
        }
    });

    group('Agent - Session End', () => {
        finishSession();
    });
}
