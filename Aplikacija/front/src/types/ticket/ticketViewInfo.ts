import type {TicketStatus} from "@/types/ticket/ticketStatus.ts";
import type {TicketPriority} from "@/types/ticket/ticketPriority.ts";
import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";
import type {NoteDetails} from "@/types/note/noteDetails.ts";
import type {MessageDetails} from "@/types/message/messageDetails.ts";

export interface TicketViewInfo {
  id: string;
  organizationId: string;
  organizationName: string;
  categoryId: string;
  categoryName: string;
  customerId: string;
  customerUsername: string;
  customerEmail: string;
  supportAgentId?: string;
  supportAgentUsername?: string;
  status: TicketStatus;
  priority: TicketPriority;
  subject: string;
  openedAt: Date;
  assignedAt?: Date;
  closedAt?: Date;
  feedback: TicketFeedback;
  lastMessageAt: Date;
  messages: MessageDetails[];
  notes: NoteDetails[];
}
