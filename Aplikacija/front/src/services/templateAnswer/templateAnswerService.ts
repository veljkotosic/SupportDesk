import { api } from '@/services/api'
import type { TemplateAnswersResult } from '@/types/templateAnswer/templateAnswersResult.ts'

const BASE_URL = '/api/v1/templateAnswer'

export const templateAnswerService = {
  async getTemplateAnswers(): Promise<TemplateAnswersResult> {
    return await api.get<TemplateAnswersResult>(`/api/v1/organization/templateAnswers`)
  },

  async addTemplateAnswer(title: string, text: string): Promise<void> {
    await api.post(BASE_URL, { title, text })
  },

  async updateTemplateAnswer(templateAnswerId: string, title: string | undefined, text: string | undefined): Promise<void> {
    await api.patch(`${BASE_URL}/${templateAnswerId}`, { title, text })
  },

  async removeTemplateAnswer(templateAnswerId: string): Promise<void> {
    await api.delete(`${BASE_URL}/${templateAnswerId}`)
  },
}
