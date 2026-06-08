<script setup lang="ts">
import { onBeforeMount, onBeforeUnmount, ref } from 'vue'
import { Check, Copy, Hash, Plus, Search, X } from 'lucide-vue-next'
import { storeToRefs } from 'pinia'
import OrganizationAdminLayout from '@/layouts/OrganizationAdminDashboardLayout.vue'
import UserAvatar from '@/components/UserAvatar.vue'
import { useOrganizationAdminAgentStore } from '@/stores/organizationAdminAgentStore.ts'

const agentStore = useOrganizationAdminAgentStore()
const { filteredAgents, searchQuery } = storeToRefs(agentStore)

const showAddModal = ref(false)
const agentEmail = ref('')
const emailError = ref('')
const inviteCode = ref('')
const copied = ref(false)
const isGeneratingCode = ref(false)

onBeforeMount(async () => {
  await agentStore.loadAgents()
  await agentStore.startLiveUpdates()
})

onBeforeUnmount(async () => {
  await agentStore.stopLiveUpdates()
  agentStore.unloadAgents()
})

function formatJoinedDate(dateInput: Date | string) {
  return new Date(dateInput).toLocaleDateString([], {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  })
}

function handleOpenModal() {
  agentEmail.value = ''
  emailError.value = ''
  inviteCode.value = ''
  copied.value = false
  showAddModal.value = true
}

function handleCloseModal() {
  showAddModal.value = false
}

function handleEmailInput() {
  emailError.value = ''
  inviteCode.value = ''
  copied.value = false
}

function validateEmail(email: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
}

async function handleGenerateCode() {
  const email = agentEmail.value.trim()

  if (!email) {
    emailError.value = 'Please enter an email address.'
    return
  }

  if (!validateEmail(email)) {
    emailError.value = 'Please enter a valid email address.'
    return
  }

  isGeneratingCode.value = true
  emailError.value = ''

  try {
    inviteCode.value = await agentStore.generateInviteCode(email)
  } catch (e: any) {
    emailError.value = e?.message ?? 'Failed to generate invite code.'
  } finally {
    isGeneratingCode.value = false
  }
}

async function handleCopyCode() {
  if (!inviteCode.value) {
    return
  }

  await navigator.clipboard.writeText(inviteCode.value)
  copied.value = true
  window.setTimeout(() => {
    copied.value = false
  }, 2000)
}

function handleChangeEmail() {
  inviteCode.value = ''
  copied.value = false
}
</script>

<template>
  <OrganizationAdminLayout active-path="/organization/agents">
    <div class="p-6 max-w-5xl mx-auto">
      <div class="flex items-center justify-between gap-4 mb-7">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">Agents</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            {{ agentStore.agents.length }} support agents
          </p>
        </div>
        <button
          type="button"
          class="flex items-center gap-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium transition-all shadow-sm"
          @click="handleOpenModal"
        >
          <Plus :size="15" />
          Add Agent
        </button>
      </div>

      <div class="relative mb-4">
        <Search :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search agents..."
          class="w-full pl-9 pr-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-900 dark:text-white placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all"
        />
      </div>

      <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 overflow-hidden">
        <div v-if="filteredAgents.length === 0" class="px-5 py-12 text-center">
          <p class="text-sm text-gray-400">No support agents found.</p>
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full">
            <thead>
              <tr class="bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
                <th class="px-5 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide">Agent</th>
                <th class="px-5 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide">Open Tickets</th>
                <th class="px-5 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide">Resolved</th>
                <th class="px-5 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide">Joined</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-50 dark:divide-gray-800">
              <tr
                v-for="agent in filteredAgents"
                :key="agent.id"
                class="hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors"
              >
                <td class="px-5 py-4">
                  <div class="flex items-center gap-3">
                    <UserAvatar :user-name="agent.userName" />
                    <div>
                      <p class="text-sm font-medium text-gray-900 dark:text-white">{{ agent.userName }}</p>
                      <p class="text-xs text-gray-500 dark:text-gray-400">{{ agent.email }}</p>
                    </div>
                  </div>
                </td>
                <td class="px-5 py-4">
                  <div class="flex items-center gap-2">
                    <span class="text-sm font-medium text-gray-900 dark:text-white">{{ agent.openTickets }}</span>
                    <div class="flex-1 max-w-16 h-1.5 rounded-full bg-gray-100 dark:bg-gray-800 overflow-hidden">
                      <div
                        class="h-full rounded-full bg-amber-400"
                        :style="{ width: `${Math.min(100, agent.openTickets * 12)}%` }"
                      />
                    </div>
                  </div>
                </td>
                <td class="px-5 py-4">
                  <span class="text-sm text-gray-700 dark:text-gray-300">{{ agent.resolvedTickets }}</span>
                </td>
                <td class="px-5 py-4">
                  <span class="text-sm text-gray-500 dark:text-gray-400">{{ formatJoinedDate(agent.joinedAt) }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-if="showAddModal" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <button class="absolute inset-0 bg-black/40 backdrop-blur-sm" aria-label="Close modal" @click="handleCloseModal" />
      <div class="relative bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 shadow-2xl w-full max-w-md p-6 z-10">
        <div class="flex items-center justify-between mb-1">
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white">Invite a new agent</h2>
          <button
            type="button"
            class="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-all"
            @click="handleCloseModal"
          >
            <X :size="16" />
          </button>
        </div>
        <p class="text-sm text-gray-500 dark:text-gray-400 mb-5">
          Generate an invite code and share it with the support agent.
        </p>

        <div>
          <label for="agent-email" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
            Agent Email <span class="text-red-400">*</span>
          </label>
          <input
            id="agent-email"
            v-model="agentEmail"
            type="email"
            placeholder="agent@company.com"
            :disabled="Boolean(inviteCode)"
            class="w-full px-4 py-2.5 rounded-xl border text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white disabled:text-gray-400 disabled:cursor-not-allowed"
            :class="emailError ? 'border-red-400 dark:border-red-600' : ''"
            @input="handleEmailInput"
          />
          <p v-if="emailError" class="text-xs text-red-500 mt-1.5">{{ emailError }}</p>
        </div>

        <button
          v-if="!inviteCode"
          type="button"
          class="w-full flex items-center justify-center gap-2 py-2.5 rounded-xl bg-blue-500 hover:bg-blue-600 disabled:bg-gray-300 text-white text-sm font-medium transition-all mt-4"
          :disabled="isGeneratingCode"
          @click="handleGenerateCode"
        >
          <Hash :size="15" />
          {{ isGeneratingCode ? 'Generating...' : 'Generate Invite Code' }}
        </button>

        <div v-else class="mt-4">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Invite Code</label>
          <div class="flex items-center justify-between gap-3 px-5 py-4 rounded-xl border border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-900/20">
            <span class="block flex-1 min-w-0 overflow-hidden text-ellipsis whitespace-nowrap text-lg font-mono font-bold tracking-wider text-blue-700 dark:text-blue-300 select-all">
              {{ inviteCode }}
            </span>
            <button
              type="button"
              class="flex-shrink-0 flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-blue-200 dark:border-blue-700 bg-white dark:bg-gray-900 text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-100 dark:hover:bg-blue-900/40 transition-all"
              @click="handleCopyCode"
            >
              <Check v-if="copied" :size="14" class="text-emerald-500" />
              <Copy v-else :size="14" />
              {{ copied ? 'Copied!' : 'Copy' }}
            </button>
          </div>
          <p class="text-xs text-gray-400 mt-2.5">
            This code expires in 72 hours.
          </p>
          <button
            type="button"
            class="mt-2 rounded-lg px-2 py-1 -ml-2 text-xs font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/30 hover:text-blue-700 dark:hover:text-blue-300 transition-colors"
            @click="handleChangeEmail"
          >
            Use a different email
          </button>
        </div>

        <button
          type="button"
          class="w-full mt-6 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 transition-all"
          @click="handleCloseModal"
        >
          Close
        </button>
      </div>
    </div>
  </OrganizationAdminLayout>
</template>
