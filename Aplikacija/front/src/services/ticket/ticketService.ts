import type {Ticket} from "@/types/ticket/ticket.ts";
import {api} from "@/services/api.ts";
import type {OpenTicketInput} from "@/types/ticket/openTicketInput.ts";

export const ticketService = {
  async getTickets(): Promise<Ticket[]> {
    const result = await api.get<any>('/api/Ticket/customerTickets');
    return result.tickets
  },

  async getTicket(ticketId: string): Promise<Ticket> {
    const result = await api.get<any>('/api/Ticket')
    return result.ticket
  },

  async createTicket(input: OpenTicketInput): Promise<string> {
    const result = await api.post<any>('/api/Ticket', input)
    return result.ticketId
  }
}
