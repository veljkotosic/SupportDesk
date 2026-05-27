<script setup lang="ts">

import {TicketStatus} from "@/types/ticket/ticketStatus.ts";

defineProps<{
  status: TicketStatus
}>()

const styles: Record<TicketStatus, string> = {
  [TicketStatus.Open]: 'bg-blue-50 text-blue-600 border border-blue-200 dark:bg-blue-900/30 dark:text-blue-400 dark:border-blue-800',
  [TicketStatus.Assigned]: 'bg-amber-50 text-amber-600 border border-amber-200 dark:bg-amber-900/30 dark:text-amber-400 dark:border-amber-800',
  [TicketStatus.Closed]: 'bg-emerald-50 text-emerald-600 border border-emerald-200 dark:bg-emerald-900/30 dark:text-emerald-400 dark:border-emerald-800',
  [TicketStatus.Archived]: 'bg-gray-100 text-gray-500 border border-gray-200 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-700',
}

const dots: Record<TicketStatus, string> = {
  [TicketStatus.Open]: 'bg-blue-500',
  [TicketStatus.Assigned]: 'bg-amber-500',
  [TicketStatus.Closed]: 'bg-emerald-500',
  [TicketStatus.Archived]: 'bg-gray-400',
}

const labels: Record<TicketStatus, string> = {
  [TicketStatus.Open]: 'Open',
  [TicketStatus.Assigned]: 'Assigned',
  [TicketStatus.Closed]: 'Closed',
  [TicketStatus.Archived]: 'Archived'
}
</script>

<template>
  <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium" :class="styles[status]">
    <span class="w-1.5 h-1.5 rounded-full" :class="dots[status]" />
    {{ labels[status] }}
  </span>
</template>
