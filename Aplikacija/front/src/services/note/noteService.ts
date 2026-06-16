import { api } from '@/services/api.ts'

export const noteService = {
  async addNote(ticketId: string, text: string): Promise<void> {
    await api.post('/api/Note', { ticketId, text })
  },
}
