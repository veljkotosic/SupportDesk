import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";

export interface TicketFeedbackInfo {
  ticketId: string;
  feedback: TicketFeedback;
}
