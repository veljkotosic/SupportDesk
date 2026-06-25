import type {OrganizationListing} from "@/types/organization/organizationListing.ts";
import {api} from "@/services/api.ts";
import type { OrganizationFaq } from '@/types/organization/organizationFaq.ts'

export const organisationService = {
  async listOrganizations(): Promise<OrganizationListing[]> {
    const result = await api.get<any>('/api/Organization/listAll')
    return result.organizations
  },

  async listFaqs(organizationId: string): Promise<OrganizationFaq[]> {
    const result = await api.get<{ faqs: OrganizationFaq[] }>(`/api/Organization/${organizationId}/faqs`)
    return result.faqs
  },
}
