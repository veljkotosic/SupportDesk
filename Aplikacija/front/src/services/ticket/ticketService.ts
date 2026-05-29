import type {Ticket} from "@/types/ticket/ticket.ts";
import {api} from "@/services/api.ts";
import type {OpenTicketInput} from "@/types/ticket/openTicketInput.ts";
import type {TicketViewInfo} from "@/types/ticket/ticketViewInfo.ts";
import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";

export const ticketService = {
  async getTickets(): Promise<Ticket[]> {
    const result = await api.get<any>('/api/Ticket/customerTickets');
    return result.tickets
  },

  async getTicket(ticketId: string): Promise<Ticket> {
    const result = await api.get<any>(`/api/Ticket/${ticketId}`)
    return result.ticket
  },

  async createTicket(input: OpenTicketInput): Promise<string> {
    const result = await api.post<any>('/api/Ticket', input)
    return result.ticketId
  },

  async getTicketViewInfo(ticketId: string) : Promise<TicketViewInfo> {
    const result = await api.get<any>(`/api/Ticket/${ticketId}/info`)
    return result.ticket
  },

  async readAllNotifications(ticketId: string) : Promise<void> {
    await api.put(`/api/Ticket/${ticketId}/readAllNotifications`)
  },

  async giveFeedback(ticketId: string, feedback: TicketFeedback) : Promise<void> {
    await api.put("/api/Ticket/feedback", { ticketId: ticketId, feedback: feedback })
  }
}
