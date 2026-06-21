import {api} from "@/services/api.ts";
import type {MessageDetails} from "@/types/message/messageDetails.ts";

export const messageService = {
  async sendMessage(ticketId: string, text: string): Promise<string> {
    const result = await api.post<any>('/api/Message', { ticketId: ticketId, text: text })
    return result.messageId
  },

  async getMessage(messageId: string): Promise<MessageDetails> {
    const result = await api.get(`/api/Message/${messageId}`)

    return result.message
  }
}
