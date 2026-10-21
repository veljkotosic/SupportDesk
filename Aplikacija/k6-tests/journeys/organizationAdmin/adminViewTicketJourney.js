import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { NoteService } from '../../services/noteService.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';

/**
 * Organization Admin View Ticket Journey (mapped from OrganizationAdminTicketDetailView.vue):
 * 1. Log in as Org Admin.
 * 2. Locate a ticket to review.
 * 3. Fetch ticket view info and review messages (view-only mode for admins).
 * 4. Add internal review note.
 * 5. Conclude session.
 */
export function adminViewTicketJourney(adminCredentials, explicitTicketId = null) {
    let targetTicketId = explicitTicketId;

    startSession(adminCredentials, 'Org Admin');

    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;

    if (!targetTicketId) {
        group('Org Admin - Locate Ticket to Review', () => {
            const skip = ((vuId * 17 + iter * 5) % 50) * 5;
            let list = TicketService.getOrganizationTickets(skip, 10);
            if (!list || !list.tickets || list.tickets.length === 0) {
                list = TicketService.getOrganizationTickets(0, 10);
            }
            if (list && list.tickets && list.tickets.length > 0) {
                const pickIdx = (vuId + iter) % list.tickets.length;
                targetTicketId = list.tickets[pickIdx].id;
            }
        });
    }

    if (targetTicketId) {
        group('Org Admin - Review Ticket Details & Messages', () => {
            TicketService.getTicketInfo(targetTicketId);
            waitBetween(3, 6);
        });

        group('Org Admin - Add Supervisory Note', () => {
            NoteService.addNote(
                targetTicketId,
                'Supervisory review: Checked ticket progress and customer correspondence.'
            );
            waitBetween(1, 3);
        });
    } else {
        waitBetween(2, 4);
    }

    group('Org Admin - Session End', () => {
        finishSession();
    });
}
