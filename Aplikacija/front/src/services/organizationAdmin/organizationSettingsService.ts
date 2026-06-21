import { api } from '@/services/api.ts'
import type { OrganizationSettings } from '@/types/organizationSettings/organizationSettings.ts'

export const organizationSettingsService = {
  async getSettings(): Promise<OrganizationSettings> {
    return await api.get<OrganizationSettings>('/api/OrganizationAdmin/settings')
  },

  async addFaq(question: string, answer: string): Promise<void> {
    await api.post('/api/Faq', { question, answer })
  },
  async updateFaq(faqId: string, question: string, answer: string): Promise<void> {
    await api.put('/api/Faq', { faqId, question, answer })
  },
  async removeFaq(faqId: string): Promise<void> {
    await api.delete('/api/Faq', { faqId })
  },

  async addTemplateAnswer(title: string, text: string): Promise<void> {
    await api.post('/api/TemplateAnswer', { title, text })
  },
  async updateTemplateAnswer(templateAnswerId: string, title: string, text: string): Promise<void> {
    await api.put('/api/TemplateAnswer', { templateAnswerId, title, text })
  },
  async removeTemplateAnswer(templateAnswerId: string): Promise<void> {
    await api.delete('/api/TemplateAnswer', { templateAnswerId })
  },

  async addCategory(name: string, description: string): Promise<void> {
    await api.post('/api/Category', { name, description })
  },
  async updateCategory(categoryId: string, name: string, description: string): Promise<void> {
    await api.put('/api/Category', { categoryId, name, description })
  },
  async removeCategory(categoryId: string): Promise<void> {
    await api.delete('/api/Category', { categoryId })
  },
}
