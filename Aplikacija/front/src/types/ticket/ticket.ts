import type {TicketStatus} from "@/types/ticket/ticketStatus.ts";
import type {TicketPriority} from "@/types/ticket/ticketPriority.ts";
import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";

export interface Ticket {
  id: string;
  organizationId: string;
  categoryId: string;
  customerId: string;
  supportAgentId: string;
  status: TicketStatus;
  priority: TicketPriority;
  subject: string;
  openedAt: Date;
  assignedAt: Date;
  closedAt?: Date;
  feedback: TicketFeedback;
}
