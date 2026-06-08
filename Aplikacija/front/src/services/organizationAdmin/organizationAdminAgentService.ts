import { api } from '@/services/api.ts'
import type { OrganizationAgentsResult } from '@/types/organizationAgent/organizationAgentsResult.ts'

const BASE_URL = '/api/OrganizationAdmin'

export const organizationAdminAgentService = {
  async getSupportAgents(): Promise<OrganizationAgentsResult> {
    return await api.get<OrganizationAgentsResult>(`${BASE_URL}/supportAgents`)
  },

  async generateInviteCode(email: string): Promise<string> {
    const result = await api.post<{ code: string }>(`${BASE_URL}/generateInviteCode`, { email })
    return result.code
  },
}
