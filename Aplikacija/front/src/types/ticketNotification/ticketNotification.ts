import type {
  TicketNotificationStatus
} from "@/types/ticketNotification/ticketNotificationStatus.ts";

export interface TicketNotification {
  id: string;
  organizationId: string;
  ticketId: string;
  text: string;
  status: TicketNotificationStatus;
  createdAt: Date;
}
