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

  async function loadTickets() {
    try {
      loadedTickets.value = await ticketService.getTickets()
    } catch (e: any) {

    }
  }

  function unloadTickets() {
    loadedTickets.value = []
    statusFilter.value = 'All'
    searchQuery.value = ''
  }

  function setStatusFilter(filter: FilterStatus) {
    statusFilter.value = filter
  }

  async function addNewlyCreatedTicket(ticketId: string) {
    const newTicket = await ticketService.getTicket(ticketId)
    loadedTickets.value.push(newTicket)
  }

  function newNotification(notification: TicketNotification) {
    const ticket = loadedTickets.value.find(t => t.id === notification.ticketId)
    if (ticket === undefined) {
      return
    }

    ticket.unreadNotifications.push(notification)
  }

  async function readNotifications(ticketId: string) {
    const ticket = loadedTickets.value.find(t => t.id === ticketId)
    if (ticket === undefined) {
      return
    }

    await ticketService.readAllNotifications(ticket.id)

    ticket.unreadNotifications = []
  }

  function assignTicket(info: TicketAssignedInfo) {
    const ticket = loadedTickets.value.find(t => t.id === info.ticketId)

    if (ticket === undefined || ticket.status !== TicketStatus.Open) {
      return
    }

    ticket.status = TicketStatus.Assigned
    ticket.assignedAt = info.assignedAt
    ticket.supportAgentId = info.supportAgentId
    ticket.supportAgentUsername = info.supportAgentUsername
  }

  function closeTicket(info: TicketClosedInfo) {
    const ticket = loadedTickets.value.find(t => t.id === info.ticketId)

    if (ticket === undefined || ticket.status !== TicketStatus.Assigned) {
      return
    }

    ticket.status = TicketStatus.Closed
    ticket.closedAt = info.closedAt
  }

  return {
    loadedTickets,
    filteredTickets,
    statusFilter,
    searchQuery,
    loadTickets,
    unloadTickets,
    setStatusFilter,
    addNewlyCreatedTicket,
    newNotification,
    readNotifications,
    assignTicket,
    closeTicket
  }
})
