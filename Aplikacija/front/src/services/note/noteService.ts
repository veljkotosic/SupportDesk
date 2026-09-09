import { api } from '@/services/api'

export const noteService = {
  async addNote(ticketId: string, text: string): Promise<void> {
    await api.post('/api/v1/note', { ticketId, text })
  },
}
