<script setup lang="ts">
import { MessageSquare } from 'lucide-vue-next'

withDefaults(
  defineProps<{
    centered?: boolean
    auth?: boolean
  }>(),
  {
    centered: false,
    auth: false,
  },
)
</script>

<template>
  <div
    class="flex items-center gap-2.5"
    :class="{ 'justify-center mb-8': centered, 'px-5 py-5 border-b border-gray-100 dark:border-gray-800': !auth }"
  >
    <div
      class="bg-blue-500 flex items-center justify-center flex-shrink-0"
      :class="auth ? 'w-9 h-9 rounded-xl' : 'w-8 h-8 rounded-lg'"
    >
      <MessageSquare :size="auth ? 18 : 16" class="text-white" />
    </div>
    <span class="font-semibold text-gray-900 dark:text-white tracking-tight" :class="auth ? 'text-lg' : 'text-base'">
      SupportDesk
    </span>
  </div>
</template>
