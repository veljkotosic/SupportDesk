import { api } from '@/services/api.ts'
import type { TemplateAnswersResult } from '@/types/templateAnswer/templateAnswersResult.ts'

export const templateAnswerService = {
  async getTemplateAnswers(): Promise<TemplateAnswersResult> {
    return await api.get<TemplateAnswersResult>('/api/TemplateAnswer')
  },
}
