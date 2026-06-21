import { api } from '@/services/api.ts'
import type { OrganizationAdminDashboardSnapshot } from '@/types/organizationAdminDashboard/organizationAdminDashboardSnapshot.ts'

const BASE_URL = '/api/OrganizationAdmin'

export const organizationAdminDashboardService = {
  async getDashboard(): Promise<OrganizationAdminDashboardSnapshot> {
    return await api.get<OrganizationAdminDashboardSnapshot>(`${BASE_URL}/dashboard`)
  },
}
