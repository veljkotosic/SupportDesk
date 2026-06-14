<script setup lang="ts">
import { computed, onBeforeMount, onBeforeUnmount, ref } from 'vue'
import { ChevronDown, ChevronLeft, ChevronRight, Search, SlidersHorizontal, X } from 'lucide-vue-next'
import CategoryBadge from '@/components/CategoryBadge.vue'
import PriorityBadge from '@/components/PriorityBadge.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import UserAvatar from '@/components/UserAvatar.vue'
import { useOrganizationTicketListStore } from '@/stores/organizationTicketListStore.ts'
import { TicketPriority } from '@/types/ticket/ticketPriority.ts'
import { TicketStatus } from '@/types/ticket/ticketStatus.ts'

defineProps<{
  title: string
  detailBasePath?: string
}>()

const ticketStore = useOrganizationTicketListStore()
const showFilters = ref(false)
let searchTimeout: ReturnType<typeof setTimeout> | undefined

const statusFilters = [
  { label: 'All', value: null, count: computed(() => ticketStore.allCount) },
  { label: 'Open', value: TicketStatus.Open, count: computed(() => ticketStore.openCount) },
  { label: 'Assigned', value: TicketStatus.Assigned, count: computed(() => ticketStore.assignedCount) },
  { label: 'Closed', value: TicketStatus.Closed, count: computed(() => ticketStore.closedCount) },
]

const priorityFilters = [
  { label: 'Critical', value: TicketPriority.Critical },
  { label: 'High', value: TicketPriority.High },
  { label: 'Medium', value: TicketPriority.Medium },
  { label: 'Low', value: TicketPriority.Low },
]

const hasFilters = computed(() =>
  ticketStore.searchQuery.trim() !== '' ||
  ticketStore.statusFilter !== null ||
  ticketStore.priorityFilter !== null,
)

onBeforeMount(async () => {
  await ticketStore.loadTickets()
  await ticketStore.startLiveUpdates()
})

onBeforeUnmount(async () => {
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }
  await ticketStore.stopLiveUpdates()
  ticketStore.clear()
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

function getDetailPath(basePath: string | undefined, ticketId: string) {
  return basePath ? `${basePath}/${ticketId}` : undefined
}

function handleSearchInput() {
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }
  searchTimeout = setTimeout(ticketStore.applyFilters, 300)
}

async function handleClearSearch() {
  ticketStore.searchQuery = ''
  await ticketStore.applyFilters()
}

async function handleStatusFilter(status: TicketStatus | null) {
  ticketStore.statusFilter = status
  await ticketStore.applyFilters()
}

async function handlePriorityFilter(priority: TicketPriority) {
  ticketStore.priorityFilter = ticketStore.priorityFilter === priority ? null : priority
  await ticketStore.applyFilters()
}

async function handleSortChange() {
  await ticketStore.applyFilters()
}

function handleToggleFilters() {
  showFilters.value = !showFilters.value
}

async function handleClearFilters() {
  ticketStore.searchQuery = ''
  ticketStore.statusFilter = null
  ticketStore.priorityFilter = null
  await ticketStore.applyFilters()
}

async function handlePreviousPage() {
  await ticketStore.goToPage(ticketStore.currentPage - 1)
}

async function handleNextPage() {
  await ticketStore.goToPage(ticketStore.currentPage + 1)
}
</script>

<template>
  <div class="p-6 max-w-7xl mx-auto">
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">{{ title }}</h1>
      <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">{{ ticketStore.totalCount }} tickets found</p>
    </div>

    <div class="flex gap-2 mb-5 overflow-x-auto pb-1">
      <button
        v-for="filter in statusFilters"
        :key="filter.label"
        type="button"
        class="px-3.5 py-1.5 rounded-lg text-sm font-medium whitespace-nowrap transition-all"
        :class="ticketStore.statusFilter === filter.value
          ? 'bg-blue-500 text-white'
          : 'bg-white dark:bg-gray-900 text-gray-600 dark:text-gray-400 border border-gray-200 dark:border-gray-700 hover:border-gray-300'"
        @click="handleStatusFilter(filter.value)"
      >
        {{ filter.label }} ({{ filter.count.value }})
      </button>
    </div>

    <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 overflow-hidden">
      <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 p-4 border-b border-gray-100 dark:border-gray-800">
        <div class="relative flex-1">
          <Search :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            v-model="ticketStore.searchQuery"
            type="text"
            placeholder="Search tickets by subject, ID, customer..."
            class="w-full pl-9 pr-9 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all"
            @input="handleSearchInput"
          />
          <button
            v-if="ticketStore.searchQuery"
            type="button"
            aria-label="Clear search"
            class="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            @click="handleClearSearch"
          >
            <X :size="14" />
          </button>
        </div>

        <button
          type="button"
          class="flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl border text-sm font-medium transition-all"
          :class="showFilters || hasFilters
            ? 'border-blue-300 dark:border-blue-700 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400'
            : 'border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-600 dark:text-gray-400 hover:border-gray-300'"
          @click="handleToggleFilters"
        >
          <SlidersHorizontal :size="15" />
          Filters
          <span
            v-if="ticketStore.priorityFilter !== null"
            class="bg-blue-500 text-white text-xs rounded-full w-4 h-4 flex items-center justify-center font-semibold"
          >
            1
          </span>
        </button>

        <div class="relative">
          <select
            v-model="ticketStore.sortBy"
            class="w-full appearance-none pl-3 pr-8 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-600 dark:text-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all cursor-pointer"
            @change="handleSortChange"
          >
            <option value="latest">Latest first</option>
            <option value="oldest">Oldest first</option>
            <option value="priority">By priority</option>
          </select>
          <ChevronDown :size="13" class="absolute right-2.5 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
        </div>
      </div>

      <div v-if="showFilters" class="px-4 py-3.5 bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
        <div class="flex flex-wrap items-end gap-6">
          <div>
            <p class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">Priority</p>
            <div class="flex gap-2 flex-wrap">
              <button
                v-for="priority in priorityFilters"
                :key="priority.label"
                type="button"
                class="px-3 py-1 rounded-lg text-xs font-medium border transition-all"
                :class="ticketStore.priorityFilter === priority.value
                  ? 'bg-blue-500 text-white border-blue-500'
                  : 'bg-white dark:bg-gray-900 text-gray-600 dark:text-gray-400 border-gray-200 dark:border-gray-700 hover:border-gray-300'"
                @click="handlePriorityFilter(priority.value)"
              >
                {{ priority.label }}
              </button>
            </div>
          </div>
          <button
            v-if="hasFilters"
            type="button"
            class="text-xs text-red-500 hover:text-red-600 font-medium flex items-center gap-1"
            @click="handleClearFilters"
          >
            <X :size="12" />
            Clear all
          </button>
        </div>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full">
          <thead>
            <tr class="bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
              <th
                v-for="heading in ['Category', 'Subject', 'Status', 'Priority', 'Customer', 'Assigned Agent', 'Last Message']"
                :key="heading"
                class="px-5 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide whitespace-nowrap"
              >
                {{ heading }}
              </th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-50 dark:divide-gray-800">
            <tr v-if="ticketStore.tickets.length === 0">
              <td colspan="7" class="px-5 py-12 text-center">
                <div class="flex flex-col items-center gap-2">
                  <Search :size="32" class="text-gray-300 dark:text-gray-600" />
                  <p class="text-gray-500 dark:text-gray-400 font-medium">No tickets found</p>
                  <p class="text-sm text-gray-400">Try adjusting your search or filters</p>
                </div>
              </td>
            </tr>
            <tr
              v-for="ticket in ticketStore.tickets"
              v-else
              :key="ticket.id"
              class="hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors"
            >
              <td class="px-5 py-3.5 whitespace-nowrap">
                <CategoryBadge :category="ticket.categoryName" />
              </td>
              <td class="px-5 py-3.5">
                <RouterLink
                  v-if="getDetailPath(detailBasePath, ticket.id)"
                  :to="getDetailPath(detailBasePath, ticket.id)!"
                  class="block text-sm text-gray-900 dark:text-white font-medium hover:text-blue-500 transition-colors line-clamp-1 max-w-[280px]"
                >
                  {{ ticket.subject }}
                </RouterLink>
                <p v-else class="text-sm text-gray-900 dark:text-white font-medium line-clamp-1 max-w-[280px]">
                  {{ ticket.subject }}
                </p>
                <p class="text-xs text-gray-400 mt-0.5 font-mono">{{ ticket.id }}</p>
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
                  <div>
                    <p class="text-sm text-gray-700 dark:text-gray-300">{{ ticket.customerUsername }}</p>
                    <p class="text-xs text-gray-400 truncate max-w-[140px]">{{ ticket.customerEmail }}</p>
                  </div>
                </div>
              </td>
              <td class="px-5 py-3.5 whitespace-nowrap">
                <span v-if="ticket.supportAgentUsername" class="text-sm text-gray-700 dark:text-gray-300">
                  {{ ticket.supportAgentUsername }}
                </span>
                <span v-else class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400">
                  Unassigned
                </span>
              </td>
              <td class="px-5 py-3.5 whitespace-nowrap">
                <span class="text-sm text-gray-500 dark:text-gray-400">{{ formatLastMessage(ticket.lastMessageAt) }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div
        v-if="ticketStore.totalCount > 0"
        class="px-5 py-3 border-t border-gray-100 dark:border-gray-800 flex items-center justify-between gap-4"
      >
        <span class="text-xs text-gray-400">
          Page {{ ticketStore.currentPage }} of {{ ticketStore.totalPages }} · {{ ticketStore.totalCount }} tickets
        </span>
        <div class="flex gap-1">
          <button
            type="button"
            class="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs border border-gray-200 dark:border-gray-700 text-gray-500 hover:bg-gray-50 dark:hover:bg-gray-800 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            :disabled="ticketStore.currentPage === 1"
            @click="handlePreviousPage"
          >
            <ChevronLeft :size="13" />
            Previous
          </button>
          <button
            type="button"
            class="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs border border-gray-200 dark:border-gray-700 text-gray-500 hover:bg-gray-50 dark:hover:bg-gray-800 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            :disabled="ticketStore.currentPage === ticketStore.totalPages"
            @click="handleNextPage"
          >
            Next
            <ChevronRight :size="13" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
