import { defineStore } from 'pinia'
import { ref } from 'vue'
import { organizationAdminDashboardService } from '@/services/organizationAdmin/organizationAdminDashboardService.ts'
import { organizationDashboardHubService } from '@/services/hubs/organizationDashboardHubService.ts'
import type { DashboardAgent } from '@/types/organizationAdminDashboard/dashboardAgent.ts'
import type { DashboardSummary } from '@/types/organizationAdminDashboard/dashboardSummary.ts'
import type { OrganizationDashboardMessageInfo } from '@/types/organizationAdminDashboard/organizationDashboardMessageInfo.ts'
import type { TicketVolumeEntry } from '@/types/organizationAdminDashboard/ticketVolumeEntry.ts'
import type { Ticket } from '@/types/ticket/ticket.ts'
import { TicketStatus } from '@/types/ticket/ticketStatus.ts'
import type { TicketAssignedInfo } from '@/types/ticket/info/ticketAssignedInfo.ts'
import type { TicketClosedInfo } from '@/types/ticket/info/ticketClosedInfo.ts'
import type { TicketFeedbackInfo } from '@/types/ticket/info/ticketFeedbackInfo.ts'

const emptySummary = (): DashboardSummary => ({
  openTickets: 0,
  assignedTickets: 0,
  resolvedTickets: 0,
  supportAgents: 0,
})

export const useOrganizationAdminDashboardStore = defineStore('organizationAdminDashboard', () => {
  const organizationName = ref('')
  const summary = ref<DashboardSummary>(emptySummary())
  const ticketVolume = ref<TicketVolumeEntry[]>([])
  const agents = ref<DashboardAgent[]>([])
  const recentTickets = ref<Ticket[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function loadDashboard() {
    isLoading.value = true
    error.value = null

    try {
      const snapshot = await organizationAdminDashboardService.getDashboard()
      organizationName.value = snapshot.organizationName
      summary.value = snapshot.summary
      ticketVolume.value = snapshot.ticketVolume
      agents.value = snapshot.agents
      recentTickets.value = snapshot.recentTickets
    } catch (e: any) {
      error.value = e?.message ?? 'Failed to load organization dashboard'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  function unloadDashboard() {
    organizationName.value = ''
    summary.value = emptySummary()
    ticketVolume.value = []
    agents.value = []
    recentTickets.value = []
    error.value = null
  }

  async function startLiveUpdates() {
    await organizationDashboardHubService.connect()

    organizationDashboardHubService.onNewTicket(addTicket)
    organizationDashboardHubService.onTicketAssigned(assignTicket)
    organizationDashboardHubService.onTicketClosed(closeTicket)
    organizationDashboardHubService.onGivenFeedback(updateFeedback)
    organizationDashboardHubService.onNewTicketMessage(updateMessageActivity)

    await organizationDashboardHubService.startLiveUpdates()
  }

  async function stopLiveUpdates() {
    organizationDashboardHubService.offAll()
    await organizationDashboardHubService.stopLiveUpdates()
    await organizationDashboardHubService.disconnect()
  }

  function addTicket(ticket: Ticket) {
    summary.value.openTickets += 1
    incrementTodayVolume('opened')
    upsertRecentTicket(ticket)
  }

  function assignTicket(info: TicketAssignedInfo) {
    summary.value.openTickets = Math.max(0, summary.value.openTickets - 1)
    summary.value.assignedTickets += 1

    const agent = agents.value.find(item => item.id === info.supportAgentId)
    if (agent) {
      agent.openTickets += 1
    }

    const ticket = recentTickets.value.find(item => item.id === info.ticketId)
    if (ticket) {
      ticket.status = TicketStatus.Assigned
      ticket.supportAgentId = info.supportAgentId
      ticket.supportAgentUsername = info.supportAgentUsername
      ticket.assignedAt = info.assignedAt
    }
  }

  function closeTicket(info: TicketClosedInfo) {
    summary.value.assignedTickets = Math.max(0, summary.value.assignedTickets - 1)
    summary.value.resolvedTickets += 1
    incrementTodayVolume('resolved')

    const ticket = recentTickets.value.find(item => item.id === info.ticketId)
    if (ticket) {
      ticket.status = TicketStatus.Closed
      ticket.closedAt = info.closedAt
    }

    const agent = agents.value.find(item => item.id === info.supportAgentId)
    if (agent) {
      agent.openTickets = Math.max(0, agent.openTickets - 1)
    }
  }

  function updateFeedback(info: TicketFeedbackInfo) {
    const ticket = recentTickets.value.find(item => item.id === info.ticketId)
    if (ticket) {
      ticket.feedback = info.feedback
    }
  }

  function updateMessageActivity(info: OrganizationDashboardMessageInfo) {
    const ticket = recentTickets.value.find(item => item.id === info.ticketId)
    if (!ticket) {
      return
    }

    ticket.lastMessageAt = info.createdAt
    upsertRecentTicket(ticket)
  }

  function upsertRecentTicket(ticket: Ticket) {
    const existingIndex = recentTickets.value.findIndex(item => item.id === ticket.id)
    if (existingIndex >= 0) {
      recentTickets.value.splice(existingIndex, 1)
    }

    recentTickets.value.unshift(ticket)
    recentTickets.value = recentTickets.value
      .sort((a, b) => new Date(b.lastMessageAt).getTime() - new Date(a.lastMessageAt).getTime())
      .slice(0, 6)
  }

  function incrementTodayVolume(field: 'opened' | 'resolved') {
    const today = new Date().toISOString().slice(0, 10)
    const entry = ticketVolume.value.find(item => item.date === today)
    if (entry) {
      entry[field] += 1
    }
  }

  return {
    organizationName,
    summary,
    ticketVolume,
    agents,
    recentTickets,
    isLoading,
    error,
    loadDashboard,
    unloadDashboard,
    startLiveUpdates,
    stopLiveUpdates,
  }
})
