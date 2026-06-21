import { api } from '@/services/api.ts'
import type { TicketPriority } from '@/types/ticket/ticketPriority.ts'
import type { TicketStatus } from '@/types/ticket/ticketStatus.ts'
import type { OrganizationTicketsPage } from '@/types/ticket/organizationTicketsPage.ts'

export interface OrganizationTicketQuery {
  skip: number
  take: number
  search: string
  status: TicketStatus | null
  priority: TicketPriority | null
  sortBy: 'latest' | 'oldest' | 'priority'
}

export const organizationTicketService = {
  async getTickets(query: OrganizationTicketQuery): Promise<OrganizationTicketsPage> {
    const parameters = new URLSearchParams({
      skip: query.skip.toString(),
      take: query.take.toString(),
      sortBy: query.sortBy,
    })

    if (query.search.trim()) {
      parameters.set('search', query.search.trim())
    }
    if (query.status !== null) {
      parameters.set('status', query.status.toString())
    }
    if (query.priority !== null) {
      parameters.set('priority', query.priority.toString())
    }

    return await api.get<OrganizationTicketsPage>(`/api/Ticket/organizationTickets?${parameters}`)
  },
}
