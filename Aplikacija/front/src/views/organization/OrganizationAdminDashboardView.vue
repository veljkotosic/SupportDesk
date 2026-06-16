<script setup lang="ts">
import { computed, onBeforeMount, onBeforeUnmount } from 'vue'
import {
  ArrowRight,
  CheckCircle,
  Clock,
  Ticket as TicketIcon,
  TrendingUp,
  Users,
} from 'lucide-vue-next'
import OrganizationAdminLayout from '@/layouts/OrganizationAdminDashboardLayout.vue'
import PriorityBadge from '@/components/PriorityBadge.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import UserAvatar from '@/components/UserAvatar.vue'
import { useOrganizationAdminDashboardStore } from '@/stores/organizationAdminDashboardStore.ts'
import { storeToRefs } from 'pinia'
import router from '@/router'

const dashboardStore = useOrganizationAdminDashboardStore()

const { organizationName, summary, ticketVolume, agents, recentTickets } = storeToRefs(dashboardStore)

const stats = computed(() => [
  {
    label: 'Open Tickets',
    value: summary.value.openTickets,
    icon: TicketIcon,
    iconClass: 'text-blue-500',
    backgroundClass: 'bg-blue-50 dark:bg-blue-900/20',
    change: '',
  },
  {
    label: 'Assigned',
    value: summary.value.assignedTickets,
    icon: Clock,
    iconClass: 'text-amber-500',
    backgroundClass: 'bg-amber-50 dark:bg-amber-900/20',
    change: '',
  },
  {
    label: 'Resolved',
    value: summary.value.resolvedTickets,
    icon: CheckCircle,
    iconClass: 'text-emerald-500',
    backgroundClass: 'bg-emerald-50 dark:bg-emerald-900/20',
    change: '',
  },
  {
    label: 'Support Agents',
    value: summary.value.supportAgents,
    icon: Users,
    iconClass: 'text-violet-500',
    backgroundClass: 'bg-violet-50 dark:bg-violet-900/20',
    change: '',
  },
])

const ticketVolumeScaleStep = computed(() => {
  const maximum = Math.max(1, ...ticketVolume.value.flatMap(entry => [entry.opened, entry.resolved]))
  return Math.ceil(maximum / 4)
})

const ticketVolumeMaximum = computed(() => {
  return ticketVolumeScaleStep.value * 4
})

const ticketVolumeScale = computed(() => {
  return Array.from({ length: 5 }, (_, index) => ticketVolumeMaximum.value - index * ticketVolumeScaleStep.value)
})

onBeforeMount(async () => {
  await dashboardStore.loadDashboard()
  await dashboardStore.startLiveUpdates()
})

onBeforeUnmount(async () => {
  await dashboardStore.stopLiveUpdates()
  dashboardStore.unloadDashboard()
})

function formatLastMessage(dateInput: Date | string) {
  return new Date(dateInput).toLocaleString([], {
    day: 'numeric',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
}

function formatVolumeDate(dateInput: string) {
  return new Date(`${dateInput}T00:00:00`).toLocaleDateString([], { weekday: 'short' })
}

function getVolumeHeight(value: number) {
  return `${Math.max(2, (value / ticketVolumeMaximum.value) * 100)}%`
}

async function handleViewAgents() {
  await router.push('/organization/agents')
}

async function handleViewTickets() {
  await router.push('/organization/tickets')
}

async function handleViewTicket(ticketId: string) {
  await router.push(`/organization/tickets/${ticketId}`)
}

</script>

<template>
  <OrganizationAdminLayout active-path="/organization/dashboard">
    <div class="p-6 max-w-7xl mx-auto">
      <div class="mb-7">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">
          {{ organizationName }}
        </h1>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
          Manage you organization here.
        </p>
      </div>

      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-7">
        <div
          v-for="stat in stats"
          :key="stat.label"
          class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-5 hover:shadow-md transition-shadow"
        >
          <div class="flex items-start justify-between mb-3">
            <div class="w-9 h-9 rounded-xl flex items-center justify-center" :class="stat.backgroundClass">
              <component :is="stat.icon" :size="18" :class="stat.iconClass" />
            </div>
            <span v-if="stat.change" class="text-xs font-medium text-emerald-500 flex items-center gap-0.5">
              <TrendingUp :size="11" />
              {{ stat.change }}
            </span>
          </div>
          <div class="text-3xl font-bold text-gray-900 dark:text-white mb-0.5">
            {{ stat.value }}
          </div>
          <div class="text-xs text-gray-500 dark:text-gray-400">
            {{ stat.label }}
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-5 mb-7">
        <div class="lg:col-span-2 bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-5">
          <div class="flex items-center justify-between mb-5">
            <div>
              <h2 class="font-semibold text-gray-900 dark:text-white">Ticket Volume</h2>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">Last 7 days</p>
            </div>
            <div class="flex gap-4 text-xs text-gray-500 dark:text-gray-400">
              <span class="flex items-center gap-1.5">
                <span class="w-2.5 h-2.5 rounded-full bg-blue-500" />
                New
              </span>
              <span class="flex items-center gap-1.5">
                <span class="w-2.5 h-2.5 rounded-full bg-emerald-500" />
                Resolved
              </span>
            </div>
          </div>

          <div
            v-if="ticketVolume.length === 0"
            class="h-44 rounded-xl border border-dashed border-gray-200 dark:border-gray-700 bg-gray-50/60 dark:bg-gray-800/30 flex items-center justify-center"
          >
            <p class="text-sm text-gray-400">Ticket volume data is not available yet.</p>
          </div>
          <div v-else class="h-44 flex gap-2">
            <div class="h-full w-6 pb-5 flex flex-col justify-between text-right">
              <span
                v-for="value in ticketVolumeScale"
                :key="value"
                class="text-[10px] leading-none text-gray-400"
              >
                {{ value }}
              </span>
            </div>
            <div class="relative flex-1 h-full">
              <div class="absolute inset-x-0 top-0 bottom-5 flex flex-col justify-between pointer-events-none">
                <div
                  v-for="value in ticketVolumeScale"
                  :key="value"
                  class="border-t border-gray-100 dark:border-gray-800"
                />
              </div>
              <div class="relative z-10 h-full flex items-end gap-3">
                <div v-for="entry in ticketVolume" :key="entry.date" class="flex-1 h-full flex flex-col justify-end gap-1">
                  <div class="flex-1 flex items-end justify-center gap-1">
                    <div
                      class="group relative w-3 bg-blue-500 hover:bg-blue-600 rounded-t transition-colors cursor-default"
                      :style="{ height: getVolumeHeight(entry.opened) }"
                      :aria-label="`${entry.opened} new tickets`"
                    >
                      <span class="absolute z-20 bottom-full left-1/2 -translate-x-1/2 mb-1.5 hidden group-hover:block whitespace-nowrap rounded-md bg-gray-900 dark:bg-gray-700 px-2 py-1 text-[10px] font-medium text-white shadow-lg">
                        New: {{ entry.opened }}
                      </span>
                    </div>
                    <div
                      class="group relative w-3 bg-emerald-500 hover:bg-emerald-600 rounded-t transition-colors cursor-default"
                      :style="{ height: getVolumeHeight(entry.resolved) }"
                      :aria-label="`${entry.resolved} resolved tickets`"
                    >
                      <span class="absolute z-20 bottom-full left-1/2 -translate-x-1/2 mb-1.5 hidden group-hover:block whitespace-nowrap rounded-md bg-gray-900 dark:bg-gray-700 px-2 py-1 text-[10px] font-medium text-white shadow-lg">
                        Resolved: {{ entry.resolved }}
                      </span>
                    </div>
                  </div>
                  <span class="text-center text-xs text-gray-400">{{ formatVolumeDate(entry.date) }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-5">
          <div class="flex items-center justify-between mb-4">
            <h2 class="font-semibold text-gray-900 dark:text-white">Agent Status</h2>
            <button
              type="button"
              class="text-xs text-blue-500 hover:text-blue-600 font-medium flex items-center gap-1"
              @click="handleViewAgents"
            >
              All
              <ArrowRight :size="12" />
            </button>
          </div>

          <div v-if="agents.length === 0" class="h-40 flex items-center justify-center text-center">
            <p class="text-sm text-gray-400">Agent status is not available yet.</p>
          </div>
          <div v-else class="space-y-3">
            <div v-for="agent in agents" :key="agent.id" class="flex items-center gap-3">
              <UserAvatar :user-name="agent.userName" />
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
                  {{ agent.userName }}
                </p>
                <p class="text-xs text-gray-500 dark:text-gray-400">Support Agent</p>
              </div>
              <div class="text-right flex-shrink-0">
                <p class="text-sm font-medium text-gray-900 dark:text-white">{{ agent.openTickets }}</p>
                <p class="text-xs text-gray-400">open</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-gray-100 dark:border-gray-800">
          <h2 class="font-semibold text-gray-900 dark:text-white">Recent Tickets</h2>
          <button
            type="button"
            class="text-xs text-blue-500 hover:text-blue-600 font-medium flex items-center gap-1"
            @click="handleViewTickets"
          >
            View all
            <ArrowRight :size="12" />
          </button>
        </div>

        <div v-if="recentTickets.length === 0" class="px-5 py-12 text-center">
          <p class="text-sm text-gray-400">Recent tickets are not available yet.</p>
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full">
            <thead>
              <tr class="bg-gray-50 dark:bg-gray-800/50">
                <th
                  v-for="heading in ['Category', 'Subject', 'Status', 'Priority', 'Customer', 'Agent', 'Last Message']"
                  :key="heading"
                  class="px-5 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide whitespace-nowrap"
                >
                  {{ heading }}
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-50 dark:divide-gray-800">
              <tr
                v-for="ticket in recentTickets"
                :key="ticket.id"
                class="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
              >
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <span class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-violet-50 text-violet-600 border border-violet-200 dark:bg-violet-900/30 dark:text-violet-400 dark:border-violet-800">
                    {{ ticket.categoryName }}
                  </span>
                </td>
                <td class="px-5 py-3.5">
                  <button
                    type="button"
                    class="text-left text-sm text-gray-900 dark:text-white font-medium hover:text-blue-500 dark:hover:text-blue-400 transition-colors line-clamp-1 max-w-xs"
                    @click="handleViewTicket(ticket.id)"
                  >
                    {{ ticket.subject }}
                  </button>
                  <p class="text-xs text-gray-400 mt-0.5">{{ ticket.id }}</p>
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <StatusBadge :status="ticket.status" />
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <PriorityBadge :priority="ticket.priority" />
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <div class="flex items-center gap-2">
                    <UserAvatar :user-name="ticket.customerUsername" size="sm" />
                    <span class="text-sm text-gray-700 dark:text-gray-300">{{ ticket.customerUsername }}</span>
                  </div>
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <span v-if="ticket.supportAgentUsername" class="text-sm text-gray-700 dark:text-gray-300">
                    {{ ticket.supportAgentUsername }}
                  </span>
                  <span v-else class="text-sm text-gray-400">Unassigned</span>
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <span class="text-sm text-gray-500 dark:text-gray-400">
                    {{ formatLastMessage(ticket.lastMessageAt) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </OrganizationAdminLayout>
</template>
