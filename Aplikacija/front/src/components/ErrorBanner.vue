<script setup lang="ts">
import { X } from 'lucide-vue-next'

defineProps<{
  message?: string | null
  dismissible?: boolean
}>()

const emit = defineEmits<{
  dismiss: []
}>()
</script>

<template>
  <div
    v-if="message"
    class="flex items-start justify-between gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-900/60 dark:bg-red-900/20 dark:text-red-300"
  >
    <p>{{ message }}</p>
    <button
      v-if="dismissible"
      type="button"
      class="mt-0.5 text-red-400 hover:text-red-600 dark:hover:text-red-200"
      aria-label="Dismiss error"
      @click="emit('dismiss')"
    >
      <X :size="14" />
    </button>
  </div>
</template>
