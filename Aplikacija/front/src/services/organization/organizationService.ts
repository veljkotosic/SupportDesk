import type {OrganizationListing} from "@/types/organization/organizationListing.ts";
import type {
  OrganizationAdminDashboardSnapshot
} from "@/types/organizationAdminDashboard/organizationAdminDashboardSnapshot";
import type {
  OrganizationKnowledgeBase
} from "@/types/organizationSettings/organizationKnowledgeBase";
import {api} from "@/services/api";
import type {OrganizationAgentsResult} from "@/types/organizationAgent/organizationAgentsResult";

const BASE_URL = `/api/v1/organization`

export const organizationService = {
  async listOrganizations(): Promise<OrganizationListing[]> {
    const result = await api.get<any>(`${BASE_URL}/all`)
    return result.organizations
  },

  async getDashboard(): Promise<OrganizationAdminDashboardSnapshot> {
    return await api.get<OrganizationAdminDashboardSnapshot>(`${BASE_URL}/dashboard`)
  },

  async getKnowledgeBase(): Promise<OrganizationKnowledgeBase> {
    return await api.get<OrganizationKnowledgeBase>(`${BASE_URL}/knowledgeBase`)
  },

  async getSupportAgents(): Promise<OrganizationAgentsResult> {
    return await api.get<OrganizationAgentsResult>(`${BASE_URL}/supportAgentsSummary`)
  },
}
