import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { TicketStatus, TicketFeedback } from '../../utils/enums.js';

/**
 * Customer Feedback Journey (mapped from feedback buttons in CustomerTicketView.vue):
 * 1. Log in as Customer.
 * 2. Find a closed ticket that has NOT been rated yet.
 * 3. Give feedback rating (Helpful / Unhelpful).
 * 4. Conclude session.
 */
export function customerFeedbackJourney(customerCredentials) {
    startSession(customerCredentials, 'Customer');

    let closedTicketId = null;

    group('Customer - Find Unrated Closed Ticket', () => {
        const vuId = typeof __VU !== 'undefined' ? __VU : 1;
        const iter = typeof __ITER !== 'undefined' ? __ITER : 0;
        const skip = ((vuId * 2 + iter) % 15);
        let list = TicketService.getCustomerTickets(skip, 10, '', TicketStatus.Closed);
        if (!list || !list.tickets || list.tickets.length === 0) {
            list = TicketService.getCustomerTickets(0, 10, '', TicketStatus.Closed);
        }
        if (list && list.tickets && list.tickets.length > 0) {
            // ONLY select a ticket that has NOT yet received feedback (Feedback == None) and is Closed
            const unrated = list.tickets.find(t => t.feedback === TicketFeedback.None && t.status === TicketStatus.Closed);
            if (unrated) {
                closedTicketId = unrated.id;
            }
        }
    });

    if (closedTicketId) {
        group('Customer - Submit Feedback', () => {
            waitBetween(1, 3);
            TicketService.giveFeedback(closedTicketId, TicketFeedback.Helpful);
        });
    }

    group('Customer - Session End', () => {
        finishSession();
    });
}
