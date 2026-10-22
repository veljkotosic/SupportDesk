import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { TicketStatus } from '../../utils/enums.js';

/**
 * Support Agent Claim Ticket Journey (mapped from OrganizationTicketList.vue + assign):
 * 1. Log in.
 * 2. Find an unassigned (Open) ticket with distributed offsets.
 * 3. Assign the ticket to self via PATCH /Ticket/{id}/assign.
 * 4. Conclude session.
 */
export function agentClaimTicketJourney(agentCredentials) {
    startSession(agentCredentials, 'Agent');

    let openTicketId = null;

    group('Agent - Locate Open Ticket', () => {
        const vuId = typeof __VU !== 'undefined' ? __VU : 1;
        const iter = typeof __ITER !== 'undefined' ? __ITER : 0;
        // Distribute across wide offsets so concurrent VUs in the same org don't target the same ticket
        const skip = ((vuId * 37 + iter * 13) % 80) * 10;
        let list = TicketService.getOrganizationTickets(skip, 10, '', TicketStatus.Open);
        if (!list || !list.tickets || list.tickets.length === 0) {
            list = TicketService.getOrganizationTickets(0, 10, '', TicketStatus.Open);
        }
        if (list && list.tickets && list.tickets.length > 0) {
            const openTickets = list.tickets.filter(t => t.status === TicketStatus.Open);
            if (openTickets.length > 0) {
                const idx = (vuId + iter) % openTickets.length;
                openTicketId = openTickets[idx].id;
            }
        }
    });

    if (openTicketId) {
        group('Agent - Claim Ticket', () => {
            waitBetween(1, 3);
            TicketService.assignTicket(openTicketId);
        });
    } else {
        waitBetween(2, 4);
    }

    group('Agent - Session End', () => {
        finishSession();
    });
}
