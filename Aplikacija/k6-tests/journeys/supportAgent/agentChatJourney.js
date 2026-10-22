import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { MessageService } from '../../services/messageService.js';
import { OrganizationService } from '../../services/organizationService.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { TicketStatus } from '../../utils/enums.js';

/**
 * Support Agent Chat Journey (mapped from SupportAgentTicketDetailView.vue):
 * Simulates active back-and-forth communication on an assigned ticket without closing it:
 * 1. Log in as Support Agent.
 * 2. Locate an assigned ticket owned by this agent (or claim an open ticket).
 * 3. Connect to ticketHubUrl and join ticket room.
 * 4. Load ticket info & available template answers.
 * 5. Think time & send reply message to customer.
 * 6. Responsive think time if real-time message frames received.
 * 7. Conclude session.
 */
export function agentChatJourney(agentCredentials, explicitTicketId = null) {
    let targetTicketId = explicitTicketId;

    const profile = startSession(agentCredentials, 'Agent');
    const agentUserId = profile && (profile.userId || profile.id);

    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;

    if (!targetTicketId) {
        group('Agent - Locate Active Ticket', () => {
            // 1. Check if this agent already has an assigned ticket in their queue
            const assignedSkip = ((vuId * 17 + iter * 7) % 10) * 10;
            let assignedList = TicketService.getOrganizationTickets(assignedSkip, 10, '', TicketStatus.Assigned);
            if (!assignedList || !assignedList.tickets || assignedList.tickets.length === 0) {
                assignedList = TicketService.getOrganizationTickets(0, 10, '', TicketStatus.Assigned);
            }
            if (assignedList && assignedList.tickets && assignedList.tickets.length > 0) {
                const myTickets = assignedList.tickets.filter(t =>
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

            // 2. If not, claim an Open ticket using distributed skip to prevent collisions
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
        let canSendMessage = false;

        group('Agent - View Ticket & Templates', () => {
            const infoRes = TicketService.getTicketInfo(targetTicketId);
            OrganizationService.getOrganizationTemplateAnswers();

            if (infoRes && infoRes.ticket) {
                const t = infoRes.ticket;
                const matchesAgent = t.supportAgentId && agentUserId &&
                    t.supportAgentId.toLowerCase() === agentUserId.toLowerCase();
                if (t.status === TicketStatus.Assigned && matchesAgent) {
                    canSendMessage = true;
                }
            }
        });

        if (canSendMessage) {
            group('Agent - Send Response Message', () => {
                waitBetween(3, 6);

                MessageService.sendMessage(
                    targetTicketId,
                    'Hello! I am currently investigating this issue and will update you shortly.'
                );
            });
        } else {
            waitBetween(2, 4);
        }
    } else {
        waitBetween(2, 5);
    }

    group('Agent - Session End', () => {
        finishSession();
    });
}
