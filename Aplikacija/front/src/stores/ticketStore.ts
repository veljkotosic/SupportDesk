import {defineStore} from "pinia";
import {computed, ref} from "vue";
import type {Ticket} from "@/types/ticket/ticket.ts";
import {ticketService} from "@/services/ticket/ticketService.ts";
import type {TicketStatus} from "@/types/ticket/ticketStatus.ts";

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

  return {
    loadedTickets,
    filteredTickets,
    statusFilter,
    searchQuery,
    loadTickets,
    unloadTickets,
    setStatusFilter,
    addNewlyCreatedTicket
  }
})
