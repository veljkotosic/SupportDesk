import type { DashboardAgent } from '@/types/organizationAdminDashboard/dashboardAgent.ts'
import type { DashboardSummary } from '@/types/organizationAdminDashboard/dashboardSummary.ts'
import type { TicketVolumeEntry } from '@/types/organizationAdminDashboard/ticketVolumeEntry.ts'
import type { Ticket } from '@/types/ticket/ticket.ts'

export interface OrganizationAdminDashboardSnapshot {
  organizationName: string
  summary: DashboardSummary
  ticketVolume: TicketVolumeEntry[]
  agents: DashboardAgent[]
  recentTickets: Ticket[]
}
