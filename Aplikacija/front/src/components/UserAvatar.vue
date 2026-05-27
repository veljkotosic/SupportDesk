<script setup lang="ts">
import {stringToColor} from "@/utility/stringToColor.ts";

withDefaults(
  defineProps<{
    userName: string
    size?: 'sm' | 'md'
  }>(),
  {
    size: 'md',
  },
)
</script>

<template>
  <div
    class="rounded-full flex items-center justify-center flex-shrink-0"
    :class="[size === 'sm' ? 'w-7 h-7' : 'w-8 h-8']"
    :style="{ backgroundColor: stringToColor(userName)}"
  >
    <span class="text-white text-xs font-semibold">{{ userName.charAt(0).toUpperCase() }}</span>
  </div>
</template>
