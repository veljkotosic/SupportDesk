import {TicketPriority} from "@/types/ticket/ticketPriority.ts";

export interface OpenTicketInput {
  organizationId: string,
  categoryId: string,
  priority: TicketPriority,
  subject: string,
  initialMessage: string,
}
