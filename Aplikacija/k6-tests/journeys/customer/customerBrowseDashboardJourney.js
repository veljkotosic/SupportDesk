import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { withSignalRHub } from '../../utils/signalR.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { TicketStatus } from '../../utils/enums.js';
import { customerDashboardHubUrl } from '../../config/environment.js';

/**
 * Customer Browse Dashboard Journey (mapped from CustomerDashboardView.vue):
 * 1. Log in.
 * 2. Connect to customerDashboardHubUrl.
 * 3. Fetch ticket list (page 1).
 * 4. Mark notifications read on visible tickets.
 * 5. Think time, then filter by Open tickets.
 * 6. Conclude session.
 */
export function customerBrowseDashboardJourney(customerCredentials) {
    startSession(customerCredentials, 'Customer');

    withSignalRHub(customerDashboardHubUrl, (socket, stats) => {
        let ticketListResult = null;

        group('Customer - Load Dashboard Tickets', () => {
            ticketListResult = TicketService.getCustomerTickets(0, 10);
        });

        group('Customer - Read Notifications', () => {
            if (ticketListResult && ticketListResult.tickets) {
                for (const ticket of ticketListResult.tickets) {
                    if (ticket.unreadNotifications && ticket.unreadNotifications.length > 0) {
                        TicketService.readAllNotifications(ticket.id);
                    }
                }
            }
        });

        group('Customer - Filter Tickets', () => {
            waitBetween(2, 4);

            TicketService.getCustomerTickets(0, 10, '', TicketStatus.Open);
        });

        if (stats.incomingCount > 0) {
            waitBetween(1, 3);
        }
    });

    group('Customer - Session End', () => {
        finishSession();
    });
}
