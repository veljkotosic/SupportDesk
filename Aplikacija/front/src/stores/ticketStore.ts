import {defineStore} from "pinia";
import {computed, ref} from "vue";
import type {Ticket} from "@/types/ticket/ticket.ts";
import {ticketService} from "@/services/ticket/ticketService.ts";
import {TicketStatus} from "@/types/ticket/ticketStatus.ts";
import type {TicketAssignedInfo} from "@/types/ticket/info/ticketAssignedInfo.ts";
import type {TicketClosedInfo} from "@/types/ticket/info/ticketClosedInfo.ts";
import type {TicketNotification} from "@/types/ticketNotification/ticketNotification.ts";

export type FilterStatus = TicketStatus | 'All'

export const useTicketStore = defineStore('ticket', () =>{
  const loadedTickets = ref<Ticket[]>([])

  const statusFilter = ref<FilterStatus>('All')
  const searchQuery = ref('')
  const currentPage = ref(1)
  const pageSize = ref(5)
  const totalCount = ref(0)
  const allCount = ref(0)
  const openCount = ref(0)
  const assignedCount = ref(0)
  const closedCount = ref(0)
  const error = ref<string | null>(null)

  const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

  const filteredTickets = computed(() => loadedTickets.value)

  async function loadTickets(page = currentPage.value) {
    error.value = null
    try {
      const nextPage = Math.min(Math.max(1, page), totalPages.value)
      const result = await ticketService.getTickets(
        (nextPage - 1) * pageSize.value,
        pageSize.value,
        searchQuery.value,
        statusFilter.value === 'All' ? null : statusFilter.value,
      )

      loadedTickets.value = result.tickets
      totalCount.value = result.totalCount
      allCount.value = result.allCount
      openCount.value = result.openCount
      assignedCount.value = result.assignedCount
      closedCount.value = result.closedCount
      currentPage.value = Math.min(nextPage, Math.max(1, Math.ceil(result.totalCount / pageSize.value)))
    } catch (e: any) {
      error.value = e?.message ?? 'Could not load tickets.'
      throw e
    }
  }

  function unloadTickets() {
    loadedTickets.value = []
    statusFilter.value = 'All'
    searchQuery.value = ''
    currentPage.value = 1
    totalCount.value = 0
    allCount.value = 0
    openCount.value = 0
    assignedCount.value = 0
    closedCount.value = 0
    error.value = null
  }

  async function setStatusFilter(filter: FilterStatus) {
    statusFilter.value = filter
    currentPage.value = 1
    await loadTickets()
  }

  async function applySearch() {
    currentPage.value = 1
    await loadTickets()
  }

  async function addNewlyCreatedTicket(ticketId: string) {
    await ticketService.getTicket(ticketId)
    await loadTickets(1)
  }

  async function goToPage(page: number) {
    if (page < 1 || page > totalPages.value || page === currentPage.value) {
      return
    }

    await loadTickets(page)
  }

  function newNotification(notification: TicketNotification) {
    const ticket = loadedTickets.value.find(t => t.id === notification.ticketId)
    if (ticket === undefined) {
      return
    }

    ticket.unreadNotifications.push(notification)
  }

  async function assignTicket(info: TicketAssignedInfo) {
    await loadTickets()
  }

  async function closeTicket(info: TicketClosedInfo) {
    await loadTickets()
  }

  return {
    loadedTickets,
    filteredTickets,
    statusFilter,
    searchQuery,
    currentPage,
    pageSize,
    totalCount,
    allCount,
    totalPages,
    openCount,
    assignedCount,
    closedCount,
    error,
    loadTickets,
    unloadTickets,
    setStatusFilter,
    applySearch,
    addNewlyCreatedTicket,
    goToPage,
    newNotification,
    assignTicket,
    closeTicket
  }
})
