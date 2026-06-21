import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { organizationTicketService } from '@/services/ticket/organizationTicketService.ts'
import { organizationDashboardHubService } from '@/services/hubs/organizationDashboardHubService.ts'
import type { OrganizationTicketListItem } from '@/types/ticket/organizationTicketListItem.ts'
import type { TicketPriority } from '@/types/ticket/ticketPriority.ts'
import type { TicketStatus } from '@/types/ticket/ticketStatus.ts'
import type { TicketFeedbackInfo } from '@/types/ticket/info/ticketFeedbackInfo.ts'

export type OrganizationTicketSort = 'latest' | 'oldest' | 'priority'

export const useOrganizationTicketListStore = defineStore('organizationTicketList', () => {
  const tickets = ref<OrganizationTicketListItem[]>([])
  const totalCount = ref(0)
  const allCount = ref(0)
  const openCount = ref(0)
  const assignedCount = ref(0)
  const closedCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(10)
  const searchQuery = ref('')
  const statusFilter = ref<TicketStatus | null>(null)
  const priorityFilter = ref<TicketPriority | null>(null)
  const sortBy = ref<OrganizationTicketSort>('latest')
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

  async function loadTickets() {
    isLoading.value = true
    error.value = null

    try {
      const result = await organizationTicketService.getTickets({
        skip: (currentPage.value - 1) * pageSize.value,
        take: pageSize.value,
        search: searchQuery.value,
        status: statusFilter.value,
        priority: priorityFilter.value,
        sortBy: sortBy.value,
      })

      tickets.value = result.tickets
      totalCount.value = result.totalCount
      allCount.value = result.allCount
      openCount.value = result.openCount
      assignedCount.value = result.assignedCount
      closedCount.value = result.closedCount
    } catch (e: any) {
      error.value = e?.message ?? 'Failed to load tickets'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function applyFilters() {
    currentPage.value = 1
    await loadTickets()
  }

  async function goToPage(page: number) {
    if (page < 1 || page > totalPages.value || page === currentPage.value) {
      return
    }

    currentPage.value = page
    await loadTickets()
  }

  async function startLiveUpdates() {
    await organizationDashboardHubService.connect()
    organizationDashboardHubService.onNewTicket(loadTickets)
    organizationDashboardHubService.onTicketAssigned(loadTickets)
    organizationDashboardHubService.onTicketClosed(loadTickets)
    organizationDashboardHubService.onNewTicketMessage(loadTickets)
    organizationDashboardHubService.onGivenFeedback(updateTicketFeedback)
    await organizationDashboardHubService.startLiveUpdates()
  }

  async function stopLiveUpdates() {
    organizationDashboardHubService.offAll()
    await organizationDashboardHubService.stopLiveUpdates()
    await organizationDashboardHubService.disconnect()
  }

  function clear() {
    tickets.value = []
    totalCount.value = 0
    allCount.value = 0
    openCount.value = 0
    assignedCount.value = 0
    closedCount.value = 0
    currentPage.value = 1
    searchQuery.value = ''
    statusFilter.value = null
    priorityFilter.value = null
    sortBy.value = 'latest'
    error.value = null
  }

  function updateTicketFeedback(info: TicketFeedbackInfo) {
    const ticket = tickets.value.find(item => item.id === info.ticketId)
    if (ticket) {
      ticket.feedback = info.feedback
    }
  }

  return {
    tickets,
    totalCount,
    allCount,
    openCount,
    assignedCount,
    closedCount,
    currentPage,
    pageSize,
    totalPages,
    searchQuery,
    statusFilter,
    priorityFilter,
    sortBy,
    isLoading,
    error,
    loadTickets,
    applyFilters,
    goToPage,
    startLiveUpdates,
    stopLiveUpdates,
    clear,
  }
})
