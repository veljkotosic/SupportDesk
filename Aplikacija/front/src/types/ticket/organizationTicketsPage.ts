import type { OrganizationTicketListItem } from '@/types/ticket/organizationTicketListItem.ts'

export interface OrganizationTicketsPage {
  tickets: OrganizationTicketListItem[]
  totalCount: number
  allCount: number
  openCount: number
  assignedCount: number
  closedCount: number
}
