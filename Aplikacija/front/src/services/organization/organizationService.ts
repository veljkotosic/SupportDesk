import type {OrganizationListing} from "@/types/organization/organizationListing.ts";
import {api} from "@/services/api.ts";

export const organisationService = {
  async listOrganizations(): Promise<OrganizationListing[]> {
    const result = await api.get<any>('/api/Organization/listAll')
    return result.organizations
  }
}
