<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { ArrowRight, MessageSquare, Moon, Sun } from 'lucide-vue-next'
import { useAppStore } from '../stores/appStore'

const appStore = useAppStore()
const { theme } = storeToRefs(appStore)

const handleToggleTheme = () => {
  appStore.toggleTheme()
}

const handleLogoClick = () => {
  // navigation will be wired later
}

const handleSignIn = () => {
  // navigation will be wired later
}

const handleGetStarted = () => {
  // navigation will be wired later
}

const handleGetStartedFree = () => {
  // action will be wired later
}

const handleCreateOrganization = () => {
  // action will be wired later
}
</script>

<template>
  <div class="flex min-h-screen flex-col bg-white text-gray-900 dark:bg-gray-950 dark:text-white">
    <nav class="sticky top-0 z-50 border-b border-gray-100 bg-white/80 backdrop-blur-md dark:border-gray-800 dark:bg-gray-950/80">
      <div class="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6">
        <button
          type="button"
          class="flex items-center gap-2.5"
          aria-label="Go to home"
          @click="handleLogoClick"
        >
          <span class="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500">
            <MessageSquare :size="16" class="text-white" />
          </span>
          <span class="text-base font-semibold tracking-tight text-gray-900 dark:text-white">
            SupportDesk
          </span>
        </button>

        <div class="flex items-center gap-2">
          <button
            type="button"
            class="rounded-lg p-2 text-gray-500 transition-all hover:bg-gray-100 dark:hover:bg-gray-800"
            aria-label="Toggle theme"
            @click="handleToggleTheme"
          >
            <Moon v-if="theme === 'light'" :size="16" />
            <Sun v-else :size="16" />
          </button>

          <button
            type="button"
            class="rounded-lg px-3 py-2 text-sm font-medium text-gray-700 transition-all hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800"
            @click="handleSignIn"
          >
            Sign In
          </button>

          <button
            type="button"
            class="rounded-lg bg-blue-500 px-4 py-2 text-sm font-medium text-white shadow-sm transition-all hover:bg-blue-600"
            @click="handleGetStarted"
          >
            Get Started
          </button>
        </div>
      </div>
    </nav>

    <main class="flex-1">
      <section class="mx-auto max-w-6xl px-4 pb-16 pt-20 text-center sm:px-6">
        <h1 class="mb-6 text-5xl font-bold leading-[1.1] tracking-tight text-gray-900 dark:text-white sm:text-6xl">
          Simple and efficient
          <br />
          <span class="text-blue-500">customer support</span>
        </h1>

        <p class="mx-auto mb-10 max-w-2xl text-lg leading-relaxed text-gray-500 dark:text-gray-400">
          SupportDesk unifies your team's customer support with smart ticketing,
          real-time chat, and multi-tenant management — all in one beautiful platform.
        </p>

        <div class="mb-6 flex flex-col items-center justify-center gap-3 sm:flex-row">
          <button
            type="button"
            class="flex items-center gap-2 rounded-xl bg-blue-500 px-6 py-3 text-sm font-medium text-white shadow-md transition-all hover:bg-blue-600 hover:shadow-lg"
            @click="handleGetStartedFree"
          >
            Get Started Free
            <ArrowRight :size="16" />
          </button>

          <button
            type="button"
            class="flex items-center gap-2 rounded-xl border border-gray-200 px-6 py-3 text-sm font-medium text-gray-700 transition-all hover:bg-gray-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-gray-900"
            @click="handleCreateOrganization"
          >
            Create Organization
          </button>
        </div>
      </section>
    </main>

    <footer class="mt-auto bg-gray-50 py-4 dark:bg-gray-900">
      <div class="mx-auto max-w-6xl px-4 text-center sm:px-6">
        <p class="text-sm text-gray-400">
          © 2026 SupportDesk. All rights reserved.
        </p>
      </div>
    </footer>
  </div>
</template>
