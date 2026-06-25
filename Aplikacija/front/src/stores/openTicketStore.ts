import {defineStore} from "pinia";
import {ref} from "vue";
import type {OrganizationListing} from "@/types/organization/organizationListing.ts";
import {organisationService} from "@/services/organization/organizationService.ts";
import {categoryService} from "@/services/category/categoryService.ts";
import type {CategoryListing} from "@/types/category/categoryListing.ts";
import type {OpenTicketInput} from "@/types/ticket/openTicketInput.ts";
import {api} from "@/services/api.ts";
import {ticketService} from "@/services/ticket/ticketService.ts";
import type { OrganizationFaq } from '@/types/organization/organizationFaq.ts'

export const useOpenTicketStore = defineStore('openTicket', () => {
  const organizationListings = ref<OrganizationListing[]>([])
  const faqs = ref<OrganizationFaq[]>([])
  const faqOrganizationId = ref<string | null>(null)
  const isFaqLoading = ref(false)
  const faqError = ref<string | null>(null)

  async function listOrganizations() {
    try {
      organizationListings.value = await organisationService.listOrganizations()
    } catch (e: any) {

    }
  }

  function unload() {
    organizationListings.value = []
    clearFaqs()
  }

  async function selectOrganization(organizationId: string) {
    clearFaqs()
    const org = organizationListings.value.find(o => o.organizationId === organizationId)
    if (org === undefined) {
      return
    }

    org.categories = await categoryService.listCategories(organizationId)
  }

  async function loadFaqs(organizationId: string) {
    if (faqOrganizationId.value === organizationId) {
      return
    }

    isFaqLoading.value = true
    faqError.value = null

    try {
      faqs.value = await organisationService.listFaqs(organizationId)
      faqOrganizationId.value = organizationId
    } catch (e: any) {
      faqs.value = []
      faqOrganizationId.value = null
      faqError.value = e?.message ?? 'Could not load FAQs.'
      throw e
    } finally {
      isFaqLoading.value = false
    }
  }

  function clearFaqs() {
    faqs.value = []
    faqOrganizationId.value = null
    isFaqLoading.value = false
    faqError.value = null
  }

  async function createTicket(input: OpenTicketInput) {
    return await ticketService.createTicket(input)
  }

  return {
    organizationListings,
    faqs,
    isFaqLoading,
    faqError,
    listOrganizations,
    selectOrganization,
    loadFaqs,
    clearFaqs,
    createTicket,
    unload,
  }
})
