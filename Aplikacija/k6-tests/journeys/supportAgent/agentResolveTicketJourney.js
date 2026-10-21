import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { MessageService } from '../../services/messageService.js';
import { NoteService } from '../../services/noteService.js';
import { OrganizationService } from '../../services/organizationService.js';
import { RECORD_SEPARATOR, withSignalRHub } from '../../utils/signalR.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { TicketStatus } from '../../utils/enums.js';
import { ticketHubUrl } from '../../config/environment.js';

/**
 * Support Agent Resolve Ticket Journey (mapped from OrganizationTicketDetail.vue):
 * 1. Log in as Support Agent.
 * 2. Locate an assigned ticket owned by this agent (or claim an open ticket).
 * 3. Connect to ticketHubUrl.
 * 4. Load full ticket details (messages & internal notes).
 * 5. Fetch organization template answers.
 * 6. Add internal investigation note.
 * 7. Send message reply to customer.
 * 8. Close ticket.
 * 9. Conclude session.
 */
export function agentResolveTicketJourney(agentCredentials, explicitTicketId = null) {
    let targetTicketId = explicitTicketId;

    const profile = startSession(agentCredentials, 'Agent');
    const agentUserId = profile && (profile.userId || profile.id);

    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;

    if (!targetTicketId) {
        group('Agent - Locate Assigned Ticket', () => {
            // 1. Check if agent already owns an assigned ticket
            const assignedSkip = ((vuId * 17 + iter * 7) % 10) * 10;
            let list = TicketService.getOrganizationTickets(assignedSkip, 10, '', TicketStatus.Assigned);
            if (!list || !list.tickets || list.tickets.length === 0) {
                list = TicketService.getOrganizationTickets(0, 10, '', TicketStatus.Assigned);
            }
            if (list && list.tickets && list.tickets.length > 0) {
                const myTickets = list.tickets.filter(t =>
                    t.status === TicketStatus.Assigned &&
                    t.supportAgentId &&
                    agentUserId &&
                    t.supportAgentId.toLowerCase() === agentUserId.toLowerCase()
                );
                if (myTickets.length > 0) {
                    const pickIdx = (vuId + iter) % myTickets.length;
                    targetTicketId = myTickets[pickIdx].id;
                }
            }

            // 2. If not, claim an open ticket using distributed skip to prevent race conditions
            if (!targetTicketId) {
                const skip = ((vuId * 37 + iter * 13) % 80) * 10;
                let openList = TicketService.getOrganizationTickets(skip, 10, '', TicketStatus.Open);
                if (!openList || !openList.tickets || openList.tickets.length === 0) {
                    openList = TicketService.getOrganizationTickets(0, 10, '', TicketStatus.Open);
                }
                if (openList && openList.tickets && openList.tickets.length > 0) {
                    const openTickets = openList.tickets.filter(t => t.status === TicketStatus.Open);
                    if (openTickets.length > 0) {
                        const idx = (vuId + iter) % openTickets.length;
                        const candidateId = openTickets[idx].id;
                        const assignRes = TicketService.assignTicket(candidateId);
                        if (assignRes && assignRes.status === 204) {
                            targetTicketId = candidateId;
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

            let canModifyAndClose = false;

            group('Agent - Inspect Ticket & Templates', () => {
                const infoRes = TicketService.getTicketInfo(targetTicketId);
                OrganizationService.getOrganizationTemplateAnswers();

                if (infoRes && infoRes.ticket) {
                    const t = infoRes.ticket;
                    const matchesAgent = t.supportAgentId && agentUserId &&
                        t.supportAgentId.toLowerCase() === agentUserId.toLowerCase();
                    if (t.status === TicketStatus.Assigned && matchesAgent) {
                        canModifyAndClose = true;
                    }
                }
            });

            if (canModifyAndClose) {
                group('Agent - Add Internal Note', () => {
                    waitBetween(2, 4);
                    NoteService.addNote(targetTicketId, 'Verified diagnostic logs. Root cause identified.');
                });

                group('Agent - Reply & Resolve Ticket', () => {
                    waitBetween(3, 5);
                    MessageService.sendMessage(
                        targetTicketId,
                        'We have resolved your issue. Please let us know if you need anything else.'
                    );

                    waitBetween(1, 2);
                    TicketService.closeTicket(targetTicketId);
                });
            } else {
                waitBetween(2, 4);
            }

            if (stats.incomingCount > 0) {
                waitBetween(1, 2);
            }
        });
    } else {
        waitBetween(2, 5);
    }

    group('Agent - Session End', () => {
        finishSession();
    });
}
