import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { MessageService } from '../../services/messageService.js';
import { OrganizationService } from '../../services/organizationService.js';
import { RECORD_SEPARATOR, withSignalRHub } from '../../utils/signalR.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { ticketHubUrl } from '../../config/environment.js';
import { TicketStatus, TicketPriority } from '../../utils/enums.js';

/**
 * Customer Chat Journey (mapped from CustomerTicketView.vue):
 * Simulates an active conversational exchange on a ticket:
 * 1. Log in as Customer.
 * 2. Find an active (Open or Assigned) ticket belonging to this customer.
 * 3. Connect to ticketHubUrl.
 * 4. Read ticket view info & mark notifications as read.
 * 5. Send message reply.
 * 6. Conclude session.
 */
export function customerChatJourney(customerCredentials, explicitTicketId = null) {
    let targetTicketId = explicitTicketId;

    startSession(customerCredentials, 'Customer');

    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;

    if (!targetTicketId) {
        group('Customer - Locate Active Ticket', () => {
            const skip = ((vuId + iter) % 10);
            // Search for Open tickets first
            let list = TicketService.getCustomerTickets(skip, 10, '', TicketStatus.Open);
            if (!list || !list.tickets || list.tickets.length === 0) {
                list = TicketService.getCustomerTickets(0, 10, '', TicketStatus.Open);
            }
            if (!list || !list.tickets || list.tickets.length === 0) {
                // Then Assigned tickets
                list = TicketService.getCustomerTickets(skip, 10, '', TicketStatus.Assigned);
            }
            if (!list || !list.tickets || list.tickets.length === 0) {
                list = TicketService.getCustomerTickets(0, 10, '', TicketStatus.Assigned);
            }

            if (list && list.tickets && list.tickets.length > 0) {
                const activeTickets = list.tickets.filter(t => t.status === TicketStatus.Open || t.status === TicketStatus.Assigned);
                if (activeTickets.length > 0) {
                    const idx = (vuId + iter) % activeTickets.length;
                    targetTicketId = activeTickets[idx].id;
                }
            }

            // Fallback: If customer has no active ticket, open one so we have an active ticket to chat on
            if (!targetTicketId) {
                const orgs = OrganizationService.getAllOrganizations();
                if (orgs && orgs.length > 0) {
                    const orgId = orgs[(vuId + iter) % orgs.length].id;
                    const cats = OrganizationService.getOrganizationCategories(orgId);
                    if (cats && cats.length > 0) {
                        const newTicket = TicketService.openTicket(
                            orgId,
                            cats[0].id,
                            TicketPriority.Medium,
                            `Chat Ticket VU${vuId}-IT${iter}`.substring(0, 40),
                            'Hello, I need assistance with this order.'
                        );
                        if (newTicket && newTicket.id) {
                            targetTicketId = newTicket.id;
                        }
                    }
                }
            }
        });
    }

    if (targetTicketId) {
        withSignalRHub(ticketHubUrl, (socket, stats) => {
            socket.send(JSON.stringify({
                type: 1,
                target: 'JoinTicket',
                arguments: [targetTicketId]
            }) + RECORD_SEPARATOR);

            group('Customer - View Ticket & Messages', () => {
                TicketService.getTicketInfo(targetTicketId);
                TicketService.readAllNotifications(targetTicketId);
            });

            group('Customer - Send Message', () => {
                waitBetween(3, 6);

                MessageService.sendMessage(
                    targetTicketId,
                    'Thank you for looking into this. Could you please give me an update?'
                );
            });

            if (stats && stats.incomingCount > 0) {
                waitBetween(2, 4);
            }
        });
    }

    group('Customer - Session End', () => {
        finishSession();
    });
}
