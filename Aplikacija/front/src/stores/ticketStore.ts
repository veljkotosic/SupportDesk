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
  const pageSize = ref(10)
  const totalCount = ref(0)
  const openCount = ref(0)
  const assignedCount = ref(0)
  const closedCount = ref(0)

  const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

  const filteredTickets = computed(() => {
    return loadedTickets.value.filter((ticket) => {
      const matchesStatus =
        statusFilter.value === 'All' || ticket.status === statusFilter.value

      const matchesSearch = ticket.subject
        .toLowerCase()
        .includes(searchQuery.value.toLowerCase())

      return matchesStatus && matchesSearch
    })
  })

  async function loadTickets(page = currentPage.value) {
    try {
      const nextPage = Math.min(Math.max(1, page), totalPages.value)
      const result = await ticketService.getTickets((nextPage - 1) * pageSize.value, pageSize.value)

      loadedTickets.value = result.tickets
      totalCount.value = result.totalCount
      openCount.value = result.openCount
      assignedCount.value = result.assignedCount
      closedCount.value = result.closedCount
      currentPage.value = Math.min(nextPage, Math.max(1, Math.ceil(result.totalCount / pageSize.value)))
    } catch (e: any) {

    }
  }

  function unloadTickets() {
    loadedTickets.value = []
    statusFilter.value = 'All'
    searchQuery.value = ''
    currentPage.value = 1
    totalCount.value = 0
    openCount.value = 0
    assignedCount.value = 0
    closedCount.value = 0
  }

  function setStatusFilter(filter: FilterStatus) {
    statusFilter.value = filter
  }

  async function addNewlyCreatedTicket(ticketId: string) {
    const newTicket = await ticketService.getTicket(ticketId)
    totalCount.value += 1
    openCount.value += 1

    if (currentPage.value === 1) {
      loadedTickets.value.unshift(newTicket)
      loadedTickets.value = loadedTickets.value.slice(0, pageSize.value)
    }
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

  function assignTicket(info: TicketAssignedInfo) {
    const ticket = loadedTickets.value.find(t => t.id === info.ticketId)

    openCount.value = Math.max(0, openCount.value - 1)
    assignedCount.value += 1

    if (ticket !== undefined && ticket.status === TicketStatus.Open) {
      ticket.status = TicketStatus.Assigned
      ticket.assignedAt = info.assignedAt
      ticket.supportAgentId = info.supportAgentId
      ticket.supportAgentUsername = info.supportAgentUsername
    }
  }

  function closeTicket(info: TicketClosedInfo) {
    const ticket = loadedTickets.value.find(t => t.id === info.ticketId)

    assignedCount.value = Math.max(0, assignedCount.value - 1)
    closedCount.value += 1

    if (ticket !== undefined && ticket.status === TicketStatus.Assigned) {
      ticket.status = TicketStatus.Closed
      ticket.closedAt = info.closedAt
    }
  }

  return {
    loadedTickets,
    filteredTickets,
    statusFilter,
    searchQuery,
    currentPage,
    pageSize,
    totalCount,
    totalPages,
    openCount,
    assignedCount,
    closedCount,
    loadTickets,
    unloadTickets,
    setStatusFilter,
    addNewlyCreatedTicket,
    goToPage,
    newNotification,
    assignTicket,
    closeTicket
  }
})
