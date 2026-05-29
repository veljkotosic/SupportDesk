import type {TicketStatus} from "@/types/ticket/ticketStatus.ts";
import type {TicketPriority} from "@/types/ticket/ticketPriority.ts";
import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";
import type {TicketNotification} from "@/types/ticketNotification/ticketNotification.ts";

export interface Ticket {
  id: string;
  organizationId: string;
  organizationName: string;
  categoryId: string;
  categoryName: string;
  customerId: string;
  customerUsername: string;
  supportAgentId: string;
  supportAgentUsername: string;
  status: TicketStatus;
  priority: TicketPriority;
  subject: string;
  openedAt: Date;
  assignedAt: Date;
  closedAt?: Date;
  feedback: TicketFeedback;

  unreadNotifications: TicketNotification[];
}
