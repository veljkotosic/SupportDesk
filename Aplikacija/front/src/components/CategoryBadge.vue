<script setup lang="ts">
defineProps<{
  category: string
}>()
</script>

<template>
  <span class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-violet-50 text-violet-600 border border-violet-200 dark:bg-violet-900/30 dark:text-violet-400 dark:border-violet-800">
    {{ category }}
  </span>
</template>
