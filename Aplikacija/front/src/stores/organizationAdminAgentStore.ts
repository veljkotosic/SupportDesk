import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { organizationAdminAgentService } from '@/services/organizationAdmin/organizationAdminAgentService.ts'
import { organizationDashboardHubService } from '@/services/hubs/organizationDashboardHubService.ts'
import type { OrganizationAgent } from '@/types/organizationAgent/organizationAgent.ts'
import type { TicketAssignedInfo } from '@/types/ticket/info/ticketAssignedInfo.ts'
import type { TicketClosedInfo } from '@/types/ticket/info/ticketClosedInfo.ts'

export const useOrganizationAdminAgentStore = defineStore('organizationAdminAgent', () => {
  const agents = ref<OrganizationAgent[]>([])
  const searchQuery = ref('')
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const filteredAgents = computed(() => {
    const query = searchQuery.value.trim().toLowerCase()
    if (!query) {
      return agents.value
    }

    return agents.value.filter(agent =>
      agent.userName.toLowerCase().includes(query) ||
      agent.email.toLowerCase().includes(query),
    )
  })

  async function loadAgents() {
    isLoading.value = true
    error.value = null

    try {
      const result = await organizationAdminAgentService.getSupportAgents()
      agents.value = result.agents
    } catch (e: any) {
      error.value = e?.message ?? 'Failed to load agents'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  function unloadAgents() {
    agents.value = []
    searchQuery.value = ''
    error.value = null
  }

  async function generateInviteCode(email: string) {
    return await organizationAdminAgentService.generateInviteCode(email)
  }

  async function startLiveUpdates() {
    await organizationDashboardHubService.connect()
    organizationDashboardHubService.onTicketAssigned(assignTicket)
    organizationDashboardHubService.onTicketClosed(closeTicket)
    await organizationDashboardHubService.startLiveUpdates()
  }

  async function stopLiveUpdates() {
    organizationDashboardHubService.offAll()
    await organizationDashboardHubService.stopLiveUpdates()
    await organizationDashboardHubService.disconnect()
  }

  function assignTicket(info: TicketAssignedInfo) {
    const agent = agents.value.find(item => item.id === info.supportAgentId)
    if (agent) {
      agent.openTickets += 1
    }
  }

  function closeTicket(info: TicketClosedInfo) {
    const agent = agents.value.find(item => item.id === info.supportAgentId)
    if (agent) {
      agent.openTickets = Math.max(0, agent.openTickets - 1)
      agent.resolvedTickets += 1
    }
  }

  return {
    agents,
    filteredAgents,
    searchQuery,
    isLoading,
    error,
    loadAgents,
    unloadAgents,
    generateInviteCode,
    startLiveUpdates,
    stopLiveUpdates,
  }
})
