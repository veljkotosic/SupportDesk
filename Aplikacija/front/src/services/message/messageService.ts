import {api} from "@/services/api";
import type {MessageDetails} from "@/types/message/messageDetails.ts";

const BASE_URL = `/api/v1/message`

export const messageService = {
  async sendMessage(ticketId: string, text: string): Promise<string> {
    const result = await api.post<any>(BASE_URL, { ticketId: ticketId, text: text })
    return result.messageId
  },

  async getMessage(messageId: string): Promise<MessageDetails> {
    return await api.get(`${BASE_URL}/${messageId}`)
  }
}
