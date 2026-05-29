<script setup lang="ts">

import {TicketPriority} from "@/types/ticket/ticketPriority.ts";

defineProps<{
  priority: TicketPriority
}>()

const styles: Record<TicketPriority, string> = {
  [TicketPriority.Critical]: 'bg-red-50 text-red-600 border border-red-200 dark:bg-red-900/30 dark:text-red-400 dark:border-red-800',
  [TicketPriority.High]: 'bg-orange-50 text-orange-600 border border-orange-200 dark:bg-orange-900/30 dark:text-orange-400 dark:border-orange-800',
  [TicketPriority.Medium]: 'bg-blue-50 text-blue-600 border border-blue-200 dark:bg-blue-900/30 dark:text-blue-400 dark:border-blue-800',
  [TicketPriority.Low]: 'bg-gray-100 text-gray-500 border border-gray-200 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-700',
}

const labels: Record<TicketPriority, string> = {
  [TicketPriority.Low]: 'Low',
  [TicketPriority.Medium]: 'Medium',
  [TicketPriority.High]: 'High',
  [TicketPriority.Critical]: 'Critical'
}
</script>

<template>
  <span class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium" :class="styles[priority]">
    {{ labels[priority] }}
  </span>
</template>
