import type {Ticket} from "@/types/ticket/ticket.ts";
import {api} from "@/services/api";
import type {OpenTicketInput} from "@/types/ticket/openTicketInput.ts";
import type {TicketViewInfo} from "@/types/ticket/ticketViewInfo.ts";
import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";
import type {CustomerTicketsPage} from "@/types/ticket/customerTicketsPage.ts";
import type {TicketStatus} from "@/types/ticket/ticketStatus.ts";
import type {OrganizationTicketsPage} from "@/types/ticket/organizationTicketsPage";
import type {TicketPriority} from "@/types/ticket/ticketPriority.ts";

export interface OrganizationTicketQuery {
  skip: number
  take: number
  searchTerm: string
  status: TicketStatus | null
  priority: TicketPriority | null
  sortBy: 'latest' | 'oldest' | 'priority'
}

export const ticketService = {
  async getOrganizationTickets(query: OrganizationTicketQuery): Promise<OrganizationTicketsPage> {
    const parameters = new URLSearchParams({
      skip: query.skip.toString(),
      take: query.take.toString(),
      sortBy: query.sortBy,
    })

    if (query.searchTerm.trim()) {
      parameters.set('searchTerm', query.searchTerm.trim())
    }
    if (query.status !== null) {
      parameters.set('status', query.status.toString())
    }
    if (query.priority !== null) {
      parameters.set('priority', query.priority.toString())
    }

    return await api.get<OrganizationTicketsPage>(`/api/v1/ticket/organization?${parameters}`)
  },

  async getCustomerTickets(skip: number, take: number, search: string, status: TicketStatus | null): Promise<CustomerTicketsPage> {
    const parameters = new URLSearchParams({
      skip: skip.toString(),
      take: take.toString(),
    })

    if (search.trim()) {
      parameters.set('searchTerm', search.trim())
    }
    if (status !== null) {
      parameters.set('status', status.toString())
    }

    return await api.get<CustomerTicketsPage>(`/api/v1/ticket/customer?${parameters}`)
  },

  async getTicket(ticketId: string): Promise<Ticket> {
    const result = await api.get<any>(`/api/v1/ticket/${ticketId}`)
    return result.ticket
  },

  async createTicket(input: OpenTicketInput): Promise<string> {
    const result = await api.post<any>('/api/v1/ticket', input)
    return result.ticketId
  },

  async getTicketViewInfo(ticketId: string) : Promise<TicketViewInfo> {
    const result = await api.get<any>(`/api/v1/ticket/${ticketId}/info`)
    return result.ticket
  },

  async readAllNotifications(ticketId: string) : Promise<void> {
    await api.patch(`/api/v1/ticket/${ticketId}/readAllNotifications`)
  },

  async giveFeedback(ticketId: string, feedback: TicketFeedback) : Promise<void> {
    await api.patch(`/api/v1/ticket/${ticketId}/giveFeedback`, { feedback: feedback })
  },

  async assignTicket(ticketId: string): Promise<void> {
    await api.patch(`/api/v1/ticket/${ticketId}/assign`)
  },

  async closeTicket(ticketId: string): Promise<void> {
    await api.patch(`/api/v1/ticket/${ticketId}/close`)
  }
}
