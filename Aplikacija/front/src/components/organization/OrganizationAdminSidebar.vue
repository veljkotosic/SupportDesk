<script setup lang="ts">
import { CircleHelp, LayoutDashboard, Ticket, Users } from 'lucide-vue-next'
import AppLogo from '@/components/AppLogo.vue'
import UserAvatar from '@/components/UserAvatar.vue'
import type { User } from '@/types/user/user.ts'

defineProps<{
  user: User
  activePath?: string
}>()

const emit = defineEmits<{
  navigate: [path: string]
}>()

const navItems = [
  { label: 'Dashboard', href: '/organization/dashboard', icon: LayoutDashboard },
  { label: 'Tickets', href: '/organization/tickets', icon: Ticket },
  { label: 'Agents', href: '/organization/agents', icon: Users },
  { label: 'FAQ', href: '/organization/faq', icon: CircleHelp },
]

function isActive(href: string, activePath?: string) {
  if (href === '/organization/dashboard') {
    return activePath === href
  }

  return activePath?.startsWith(href)
}

function handleNavigate(path: string) {
  emit('navigate', path)
}
</script>

<template>
  <div class="flex flex-col h-full">
    <AppLogo />
    <nav class="flex-1 px-3 py-4 space-y-0.5 overflow-y-auto">
      <a
        v-for="item in navItems"
        :key="item.href"
        href="#"
        class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-all duration-150 group"
        :class="
          isActive(item.href, activePath)
            ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 font-medium'
            : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-gray-900 dark:hover:text-gray-100'
        "
        @click.prevent="handleNavigate(item.href)"
      >
        <span
          class="transition-colors"
          :class="
            isActive(item.href, activePath)
              ? 'text-blue-500'
              : 'text-gray-400 group-hover:text-gray-600 dark:group-hover:text-gray-300'
          "
        >
          <component :is="item.icon" :size="18" />
        </span>
        {{ item.label }}
        <span v-if="isActive(item.href, activePath)" class="ml-auto w-1.5 h-1.5 rounded-full bg-blue-500" />
      </a>
    </nav>

    <div class="border-t border-gray-100 dark:border-gray-800 p-3">
      <div class="flex items-center gap-3 px-3 py-2">
        <UserAvatar :user-name="user.userName" />
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
            {{ user.userName }}
          </p>
          <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
            {{ user.email }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
