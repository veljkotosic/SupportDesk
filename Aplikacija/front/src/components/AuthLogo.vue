<script setup lang="ts">
import { MessageSquare } from 'lucide-vue-next'
</script>

<template>
  <div class="flex items-center justify-center gap-2.5 mb-8">
    <div class="w-9 h-9 rounded-xl bg-blue-500 flex items-center justify-center">
      <MessageSquare :size="18" class="text-white" />
    </div>
    <span class="font-semibold text-lg text-gray-900 dark:text-white tracking-tight">
      SupportDesk
    </span>
  </div>
</template>
