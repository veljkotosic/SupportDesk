import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { MessageService } from '../../services/messageService.js';
import { OrganizationService } from '../../services/organizationService.js';
import { waitBetween } from '../../utils/wait.js';
import { initSession, finishSession } from '../shared/session.js';
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

    initSession(customerCredentials, 'Customer');

    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;

    if (!targetTicketId) {
        group('Customer - Locate Active Ticket', () => {
            // Search for Open tickets first, then Assigned
            let list = TicketService.getCustomerTickets(0, 10, '', TicketStatus.Open);
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
                        if (newTicket && (newTicket.ticketId || newTicket.id)) {
                            targetTicketId = newTicket.ticketId || newTicket.id;
                        }
                    }
                }
            }
        });
    }

    if (targetTicketId) {
        let canSendMessage = false;

        group('Customer - View Ticket & Messages', () => {
            const infoRes = TicketService.getTicketInfo(targetTicketId);
            TicketService.readAllNotifications(targetTicketId);

            if (infoRes && infoRes.ticket) {
                const t = infoRes.ticket;
                // Customer can send messages only if status is Open or Assigned (never Closed)
                if (t.status === TicketStatus.Open || t.status === TicketStatus.Assigned) {
                    canSendMessage = true;
                }
            }
        });

        if (canSendMessage) {
            group('Customer - Send Message', () => {
                waitBetween(3, 6);

                MessageService.sendMessage(
                    targetTicketId,
                    'Thank you for looking into this. Could you please give me an update?'
                );
            });
        } else {
            waitBetween(2, 4);
        }
    } else {
        waitBetween(2, 4);
    }

    group('Customer - Session End', () => {
        finishSession();
    });
}
