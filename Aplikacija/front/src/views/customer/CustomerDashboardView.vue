<script setup lang="ts">
import {computed, onBeforeMount, onUnmounted} from 'vue'
import { CheckCircle, ChevronLeft, ChevronRight, Clock, MessageSquare, Plus, Search, Ticket } from 'lucide-vue-next'
import CustomerLayout from '../../layouts/CustomerDashboardLayout.vue'
import StatusBadge from "@/components/StatusBadge.vue";
import PriorityBadge from "@/components/PriorityBadge.vue";
import CategoryBadge from "@/components/CategoryBadge.vue";
import ErrorBanner from '@/components/ErrorBanner.vue'
import {type FilterStatus, useTicketStore} from "@/stores/ticketStore.ts";
import {TicketStatus} from "@/types/ticket/ticketStatus.ts";
import type {Ticket as TicketEntity} from "@/types/ticket/ticket.ts";
import {customerDashboardHubService} from "@/services/hubs/customerDashboardHubService.ts";
import router from "@/router";
import {useAuthStore} from "@/stores/authStore.ts";

const authStore = useAuthStore()
const ticketStore = useTicketStore()
let searchTimeout: ReturnType<typeof setTimeout> | undefined

const ticketStatusFilters: { label: string; value: FilterStatus }[] = [
  { label: 'All', value: 'All' },
  { label: 'Open', value: TicketStatus.Open },
  { label: 'Assigned', value: TicketStatus.Assigned },
  { label: 'Closed', value: TicketStatus.Closed },
]

onBeforeMount(async () => {
  try {
    await ticketStore.loadTickets()
  } catch (e: any) {

  }
  await customerDashboardHubService.connect()
  await customerDashboardHubService.startLiveUpdates()
  customerDashboardHubService.onTicketAssigned((info) => {
    ticketStore.assignTicket(info)
  })
  customerDashboardHubService.onTicketClosed((info) => {
    ticketStore.closeTicket(info)
  })
  customerDashboardHubService.onTicketNotification((notification) => {
    ticketStore.newNotification(notification)
  })
})

onUnmounted(async () => {
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }
  ticketStore.unloadTickets()
  customerDashboardHubService.offAll()
  await customerDashboardHubService.stopLiveUpdates()
  await customerDashboardHubService.disconnect()
})

const stats = computed(() => [
  {
    label: 'Total Tickets',
    value: ticketStore.allCount,
    icon: Ticket,
    iconClass: 'text-blue-500',
    bg: 'bg-blue-50 dark:bg-blue-900/20',
  },
  {
    label: 'Open',
    value: ticketStore.openCount + ticketStore.assignedCount,
    icon: Clock,
    iconClass: 'text-amber-500',
    bg: 'bg-amber-50 dark:bg-amber-900/20',
  },
  {
    label: 'Resolved',
    value: ticketStore.closedCount,
    icon: CheckCircle,
    iconClass: 'text-emerald-500',
    bg: 'bg-emerald-50 dark:bg-emerald-900/20',
  },
])

const getNotificationLabel = (ticket: TicketEntity) => {
  const count = ticket.unreadNotifications.length;
  if (count === 1) {
    return ticket.unreadNotifications[0]!.text;
  }
  if (count > 1) {
    return `You have ${count} new notifications.`;
  }
  return '';
};

async function handleTicketRoute(ticketId: string) {
  await router.push(`/customer/ticket/${ticketId}`)
}

async function handleFilterChange(nextFilter: FilterStatus) {
  try {
    await ticketStore.setStatusFilter(nextFilter)
  } catch (e: any) {

  }
}

function handleSearchInput() {
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }

  searchTimeout = setTimeout(() => {
    ticketStore.applySearch().catch(() => {})
  }, 300)
}

async function handlePreviousPage() {
  try {
    await ticketStore.goToPage(ticketStore.currentPage - 1)
  } catch (e: any) {

  }
}

async function handleNextPage() {
  try {
    await ticketStore.goToPage(ticketStore.currentPage + 1)
  } catch (e: any) {

  }
}
</script>

<template>
  <CustomerLayout activePath="/customer/dashboard">
    <div class="p-6 max-w-4xl mx-auto">
      <div class="flex items-start justify-between gap-4 mb-7">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">
            Hello, {{ authStore.user!.userName }}
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Track and manage your support requests
          </p>
        </div>
        <RouterLink
          to="/customer/openTicket"
          class="flex items-center gap-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium transition-all shadow-sm flex-shrink-0"
        >
          <Plus :size="15" />
          New Ticket
        </RouterLink>
      </div>

      <div class="grid grid-cols-3 gap-4 mb-7">
        <div
          v-for="stat in stats"
          :key="stat.label"
          class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4"
        >
          <div class="w-8 h-8 rounded-xl flex items-center justify-center mb-3" :class="stat.bg">
            <component :is="stat.icon" :size="16" :class="stat.iconClass" />
          </div>
          <div class="text-2xl font-bold text-gray-900 dark:text-white">
            {{ stat.value }}
          </div>
          <div class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
            {{ stat.label }}
          </div>
        </div>
      </div>

      <div class="flex flex-col sm:flex-row gap-3 mb-4">
        <div class="relative flex-1">
          <Search :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            v-model="ticketStore.searchQuery"
            type="text"
            placeholder="Search your tickets..."
            class="w-full pl-9 pr-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-900 dark:text-white placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all"
            @input="handleSearchInput"
          />
        </div>
        <div class="flex gap-2 overflow-x-auto">
          <button
            v-for="filterInput in ticketStatusFilters"
            :key="filterInput.label"
            type="button"
            class="px-3.5 py-2 rounded-xl text-sm font-medium whitespace-nowrap transition-all"
            :class="
              ticketStore.statusFilter === filterInput.value
                ? 'bg-blue-500 text-white'
                : 'bg-white dark:bg-gray-900 text-gray-600 dark:text-gray-400 border border-gray-200 dark:border-gray-700 hover:border-gray-300'
            "
            @click="handleFilterChange(filterInput.value)"
          >
            {{ filterInput.label }}
          </button>
        </div>
      </div>

      <ErrorBanner class="mb-4" :message="ticketStore.error" />

      <div class="space-y-3">
        <div
          v-if="ticketStore.filteredTickets.length === 0"
          class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-10 text-center"
        >
          <div class="w-12 h-12 rounded-2xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center mx-auto mb-3">
            <Ticket :size="22" class="text-gray-400" />
          </div>
          <p class="font-medium text-gray-700 dark:text-gray-300 mb-1">No tickets yet</p>
          <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
            Create your first ticket to get support from our team.
          </p>
          <RouterLink
            to="/customer/openTicket"
            class="inline-flex items-center gap-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium transition-all"
          >
            <Plus :size="15" />
            Create Ticket
          </RouterLink>
        </div>

        <template v-else>
          <a
            v-for="ticket in ticketStore.filteredTickets"
            :key="ticket.id"
            href="#"
            class="block bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 hover:border-blue-200 dark:hover:border-blue-800 hover:shadow-md transition-all p-4 group"
            :class="[
              ticket.unreadNotifications.length > 0
                ? 'border-blue-400 dark:border-blue-500 shadow-md ring-1 ring-blue-100 dark:ring-blue-900/30 bg-blue-50/30 dark:bg-blue-900/10'
                : 'border-gray-100 dark:border-gray-800 hover:border-blue-200 dark:hover:border-blue-800 hover:shadow-md'
            ]"
            @click.prevent="handleTicketRoute(ticket.id)"
          >
            <div class="flex items-start gap-3">
              <div class="w-9 h-9 rounded-xl bg-blue-50 dark:bg-blue-900/20 flex items-center justify-center flex-shrink-0">
                <MessageSquare :size="16" class="text-blue-500" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-start justify-between gap-3">
                  <div class="min-w-0">
                    <p class="text-sm font-medium text-gray-900 dark:text-white group-hover:text-blue-500 transition-colors truncate">
                      {{ ticket.subject }}
                    </p>
                    <div class="flex items-center gap-2 mt-1.5 flex-wrap">
                      <StatusBadge :status="ticket.status" />
                      <PriorityBadge :priority="ticket.priority" />
                      <CategoryBadge :category="ticket.categoryName" />
                    </div>
                  </div>
                  <div class="text-right flex-shrink-0">
                    <p class="text-xs text-gray-400 font-mono">{{ ticket.id }}</p>
                  </div>
                </div>

                <div class="flex items-center gap-3 mt-2.5 pt-2.5 border-t border-gray-50 dark:border-gray-800">
                  <p v-if="ticket.supportAgentUsername" class="text-xs text-gray-500 dark:text-gray-400">
                    Agent:
                    <span class="text-gray-700 dark:text-gray-300 font-medium">
                      {{ ticket.supportAgentUsername }}
                    </span>
                  </p>
                  <p v-else class="text-xs text-gray-400">Awaiting assignment</p>
                  <p class="text-xs text-gray-400">{{ ticket.organizationName }}</p>

                  <div v-if="ticket.unreadNotifications.length > 0" class="flex-grow flex items-center gap-2">
                    <span class="relative flex h-2 w-2">
                      <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75"></span>
                      <span class="relative inline-flex rounded-full h-2 w-2 bg-blue-500"></span>
                    </span>

                    <p class="text-xs font-semibold text-blue-600 dark:text-blue-400 italic">
                      {{ getNotificationLabel(ticket) }}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </a>
        </template>
      </div>

      <div
        v-if="ticketStore.totalCount > 0"
        class="flex items-center justify-between gap-4 mt-5 px-1"
      >
        <p class="text-xs text-gray-500 dark:text-gray-400">
          Page {{ ticketStore.currentPage }} of {{ ticketStore.totalPages }}
          · {{ ticketStore.totalCount }} tickets
        </p>
        <div class="flex items-center gap-2">
          <button
            type="button"
            class="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-medium border border-gray-200 dark:border-gray-700 text-gray-600 dark:text-gray-300 hover:bg-white dark:hover:bg-gray-900 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
            :disabled="ticketStore.currentPage === 1"
            @click="handlePreviousPage"
          >
            <ChevronLeft :size="14" />
            Previous
          </button>
          <button
            type="button"
            class="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-medium border border-gray-200 dark:border-gray-700 text-gray-600 dark:text-gray-300 hover:bg-white dark:hover:bg-gray-900 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
            :disabled="ticketStore.currentPage === ticketStore.totalPages"
            @click="handleNextPage"
          >
            Next
            <ChevronRight :size="14" />
          </button>
        </div>
      </div>
    </div>
  </CustomerLayout>
</template>
