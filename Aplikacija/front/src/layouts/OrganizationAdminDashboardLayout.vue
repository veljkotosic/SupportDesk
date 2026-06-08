<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { ChevronDown, LogOut, Menu, Moon, Sun, X } from 'lucide-vue-next'
import { storeToRefs } from 'pinia'
import OrganizationAdminSidebar from '@/components/organization/OrganizationAdminSidebar.vue'
import UserAvatar from '@/components/UserAvatar.vue'
import { useAuthStore } from '@/stores/authStore.ts'
import { useAppStore } from '@/stores/appStore.ts'
import router from '@/router'

withDefaults(
  defineProps<{
    activePath?: string
  }>(),
  {
    activePath: '/organization/dashboard',
  },
)

const appStore = useAppStore()
const authStore = useAuthStore()

const { theme } = storeToRefs(appStore)
const user = authStore.user!

const sidebarOpen = ref(false)
const userMenuOpen = ref(false)
const userMenuRef = ref<HTMLElement | null>(null)

function openSidebar() {
  sidebarOpen.value = true
}

function closeSidebar() {
  sidebarOpen.value = false
}

function toggleUserMenu() {
  userMenuOpen.value = !userMenuOpen.value
}

function handleClickOutside(event: MouseEvent) {
  if (userMenuOpen.value && userMenuRef.value && !userMenuRef.value.contains(event.target as Node)) {
    userMenuOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

async function handleNavigate(path: string) {
  sidebarOpen.value = false
  await router.push(path)
}

function handleThemeToggle() {
  appStore.toggleTheme()
}

async function handleLogout() {
  try {
    await authStore.logout()
  } catch (e: any) {

  }
}
</script>

<template>
  <div class="flex h-screen bg-gray-50 dark:bg-gray-950 overflow-hidden">
    <aside class="hidden lg:flex w-60 flex-col bg-white dark:bg-gray-900 border-r border-gray-100 dark:border-gray-800 flex-shrink-0">
      <OrganizationAdminSidebar :user="user" :active-path="activePath" @navigate="handleNavigate" />
    </aside>

    <div v-if="sidebarOpen" class="lg:hidden fixed inset-0 z-50 flex">
      <button class="fixed inset-0 bg-black/40 backdrop-blur-sm" aria-label="Close sidebar" @click="closeSidebar" />
      <aside class="relative w-64 bg-white dark:bg-gray-900 shadow-xl z-10">
        <button class="absolute top-4 right-4 p-1 text-gray-400 hover:text-gray-600" @click="closeSidebar">
          <X :size="18" />
        </button>
        <OrganizationAdminSidebar :user="user" :active-path="activePath" @navigate="handleNavigate" />
      </aside>
    </div>

    <div class="flex-1 flex flex-col min-w-0 overflow-hidden">
      <header class="h-14 bg-white dark:bg-gray-900 border-b border-gray-100 dark:border-gray-800 flex items-center px-4 gap-3 flex-shrink-0">
        <button
          class="lg:hidden p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800"
          aria-label="Open sidebar"
          @click="openSidebar"
        >
          <Menu :size="18" />
        </button>

        <div class="flex-1" />

        <button
          class="p-2 rounded-lg text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-all"
          aria-label="Toggle theme"
          @click="handleThemeToggle"
        >
          <Moon v-if="theme === 'light'" :size="16" />
          <Sun v-else :size="16" />
        </button>

        <div ref="userMenuRef" class="relative">
          <button
            class="flex items-center gap-2 px-2.5 py-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-all"
            @click="toggleUserMenu"
          >
            <UserAvatar :user-name="user.userName" size="sm" />
            <div class="hidden sm:block text-left">
              <p class="text-sm font-medium text-gray-900 dark:text-white leading-tight">
                {{ user.userName }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400">Organization Admin</p>
            </div>
            <ChevronDown :size="14" class="text-gray-400" />
          </button>

          <div
            v-if="userMenuOpen"
            class="absolute right-0 top-full mt-1.5 w-52 bg-white dark:bg-gray-900 rounded-xl shadow-lg border border-gray-100 dark:border-gray-800 py-1 z-50"
          >
            <button
              class="w-full px-3 py-2 text-sm text-left text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 flex items-center gap-2"
              @click="handleLogout"
            >
              <LogOut :size="16" class="text-gray-400" />
              <span>Log out</span>
            </button>
          </div>
        </div>
      </header>

      <main class="flex-1 overflow-auto">
        <slot />
      </main>
    </div>
  </div>
</template>
