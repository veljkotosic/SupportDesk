import {defineStore} from "pinia";
import {ref} from "vue";
import type {OrganizationListing} from "@/types/organization/organizationListing.ts";
import {organisationService} from "@/services/organization/organizationService.ts";
import {categoryService} from "@/services/category/categoryService.ts";
import type {CategoryListing} from "@/types/category/categoryListing.ts";
import type {OpenTicketInput} from "@/types/ticket/openTicketInput.ts";
import {api} from "@/services/api.ts";
import {ticketService} from "@/services/ticket/ticketService.ts";

export const useOpenTicketStore = defineStore('openTicket', () => {
  const organizationListings = ref<OrganizationListing[]>([])

  async function listOrganizations() {
    try {
      organizationListings.value = await organisationService.listOrganizations()
    } catch (e: any) {

    }
  }

  function unload() {
    organizationListings.value = []
  }

  async function selectOrganization(organizationId: string) {
    const org = organizationListings.value.find(o => o.organizationId === organizationId)
    if (org === undefined) {
      return
    }

    org.categories = await categoryService.listCategories(organizationId)
  }

  async function createTicket(input: OpenTicketInput) {
    return await ticketService.createTicket(input)
  }

  return {
    organizationListings,
    listOrganizations,
    selectOrganization,
    createTicket,
    unload,
  }
})
