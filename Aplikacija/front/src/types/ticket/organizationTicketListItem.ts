import type { TicketPriority } from '@/types/ticket/ticketPriority.ts'
import type { TicketStatus } from '@/types/ticket/ticketStatus.ts'
import type { TicketFeedback } from '@/types/ticket/ticketFeedback.ts'

export interface OrganizationTicketListItem {
  id: string
  categoryId: string
  categoryName: string
  customerId: string
  customerUsername: string
  customerEmail: string
  supportAgentId?: string
  supportAgentUsername?: string
  status: TicketStatus
  priority: TicketPriority
  feedback: TicketFeedback
  subject: string
  lastMessageAt: Date | string
}
