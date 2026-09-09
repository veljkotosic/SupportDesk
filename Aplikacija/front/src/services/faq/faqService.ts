import {api} from "@/services/api";
import type {OrganizationFaq} from "@/types/organization/organizationFaq";

const BASE_URL = '/api/v1/faq'

export const faqService = {
  async addFaq(question: string, answer: string): Promise<void> {
    await api.post(BASE_URL, { question, answer })
  },

  async updateFaq(faqId: string, question: string, answer: string): Promise<void> {
    await api.patch(`${BASE_URL}/${faqId}`, { question, answer })
  },

  async removeFaq(faqId: string): Promise<void> {
    await api.delete(`${BASE_URL}/${faqId}`)
  },

  async listFaqs(organizationId: string): Promise<OrganizationFaq[]> {
    const result = await api.get<{ faqs: OrganizationFaq[] }>(`/api/v1/organization/${organizationId}/faqs`)
    return result.faqs
  },
}
