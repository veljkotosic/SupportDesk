import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { withSignalRHub } from '../../utils/signalR.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { TicketStatus, TicketPriority, TicketSortOption } from '../../utils/enums.js';
import { organizationDashboardHubUrl } from '../../config/environment.js';

/**
 * Organization Admin Browse Tickets Journey (mapped from OrganizationAdminTicketsView.vue):
 * 1. Log in as Org Admin.
 * 2. Connect to organizationDashboardHubUrl.
 * 3. Load default queue (page 1, Latest).
 * 4. Filter by Open tickets.
 * 5. Filter by High Priority sorted by Priority.
 * 6. Browse pagination (page 2).
 * 7. Conclude session.
 */
export function adminBrowseTicketsJourney(adminCredentials) {
    startSession(adminCredentials, 'Org Admin');

    withSignalRHub(organizationDashboardHubUrl, (socket, stats) => {
        group('Org Admin - Load Default Ticket Queue', () => {
            TicketService.getOrganizationTickets(0, 10, '', null, null, TicketSortOption.Latest);
        });

        group('Org Admin - Filter by Open Tickets', () => {
            waitBetween(2, 4);
            TicketService.getOrganizationTickets(0, 10, '', TicketStatus.Open, null, TicketSortOption.Latest);
        });

        group('Org Admin - Filter High Priority', () => {
            waitBetween(1, 3);
            TicketService.getOrganizationTickets(0, 10, '', null, TicketPriority.High, TicketSortOption.Priority);
        });

        group('Org Admin - Browse Page 2', () => {
            waitBetween(1, 2);
            TicketService.getOrganizationTickets(10, 10, '', null, null, TicketSortOption.Latest);
        });

        if (stats && stats.incomingCount > 0) {
            waitBetween(1, 2);
        }
    });

    group('Org Admin - Session End', () => {
        finishSession();
    });
}
