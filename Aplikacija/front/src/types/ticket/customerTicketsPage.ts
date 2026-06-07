import type { Ticket } from '@/types/ticket/ticket.ts'

export interface CustomerTicketsPage {
  tickets: Ticket[]
  totalCount: number
  openCount: number
  assignedCount: number
  closedCount: number
}
