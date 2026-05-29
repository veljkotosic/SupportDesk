<script setup lang="ts">
import {computed, onMounted, onUnmounted, reactive, ref} from 'vue'
import { ArrowLeft, ChevronDown, Send } from 'lucide-vue-next'
import CustomerLayout from '../../layouts/CustomerDashboardLayout.vue'
import {TicketPriority} from "@/types/ticket/ticketPriority.ts";
import {useOpenTicketStore} from "@/stores/openTicketStore.ts";
import router from "@/router";
import type {OpenTicketInput} from "@/types/ticket/openTicketInput.ts";
import {storeToRefs} from "pinia";
import {useTicketStore} from "@/stores/ticketStore.ts";

const openTicketStore = useOpenTicketStore()
const ticketStore = useTicketStore()

const {organizationListings} = storeToRefs(openTicketStore)

const priorities = Object.keys(TicketPriority)
  .filter(k => isNaN(Number(k)))
  .map(key => ({
    label: key,
    value: TicketPriority[key as keyof typeof TicketPriority]
  }))

const form = reactive<OpenTicketInput>({
  organizationId: '',
  categoryId: '',
  subject: '',
  priority: TicketPriority.Medium,
  initialMessage: '',
})

const submitted = ref(false)

const selectedOrganization = computed(() =>
  organizationListings.value.find((organization) => organization.organizationId === form.organizationId),
)

const canSubmit = computed(
  () => Boolean(form.organizationId && form.categoryId && form.subject && form.initialMessage),
)

const priorityColors: Record<TicketPriority, string> = {
  [TicketPriority.Low]: 'border-gray-300 bg-gray-50 dark:bg-gray-800 text-gray-600 dark:text-gray-400',
  [TicketPriority.Medium]: 'border-blue-400 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400',
  [TicketPriority.High]: 'border-orange-400 bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400',
  [TicketPriority.Critical]: 'border-red-400 bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400',
}

onMounted(() => {
  openTicketStore.listOrganizations()
})

onUnmounted(() => {
  openTicketStore.unload()
})

function handleBackRoute() {
  router.back()
}

async function handleOrganizationChange() {
  form.categoryId = ''
  await openTicketStore.selectOrganization(form.organizationId)
}

function handleCategorySelect(categoryId: string) {
  form.categoryId = categoryId
}

function handlePrioritySelect(priority: TicketPriority) {
  form.priority = priority
}

async function handleSubmit() {
  if (!canSubmit.value) return

  submitted.value = true
  try {
    const ticketId = await openTicketStore.createTicket(form)
    await ticketStore.addNewlyCreatedTicket(ticketId)
    await router.push(`/customer/ticket/${ticketId}`)
  } catch (e: any) {

  }
}

function handleCancel() {
  router.back()
}
</script>

<template>
  <CustomerLayout active-path="/customer/openTicket">
    <div class="p-6 max-w-2xl mx-auto">
      <div class="flex items-center gap-3 mb-7">
        <RouterLink
          to=""
          class="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 transition-all"
          @click.prevent="handleBackRoute"
        >
          <ArrowLeft :size="17" />
        </RouterLink>
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">
            Open New Ticket
          </h1>
        </div>
      </div>

      <div
        v-if="submitted"
        class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-10 text-center"
      >
        <div class="w-14 h-14 rounded-full bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center mx-auto mb-4">
          <Send :size="24" class="text-emerald-500" />
        </div>
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
          Ticket submitted!
        </h2>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          Your ticket has been received. Our support team will get back to you shortly.
        </p>
      </div>

      <form v-else @submit.prevent="handleSubmit">
        <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-6 space-y-5">
          <div>
            <label
              for="ticket-organization"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
            >
              Organization <span class="text-red-400">*</span>
            </label>
            <div class="relative">
              <select
                id="ticket-organization"
                v-model="form.organizationId"
                required
                class="w-full appearance-none px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all pr-10"
                @change="handleOrganizationChange"
              >
                <option value="">Select an organization...</option>
                <option
                  v-for="organization in organizationListings"
                  :key="organization.organizationId"
                  :value="organization.organizationId"
                >
                  {{ organization.organizationName }}
                </option>
              </select>
              <ChevronDown :size="15" class="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
              Category <span class="text-red-400">*</span>
            </label>
            <div
              v-if="!form.organizationId"
              class="px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800 text-sm text-gray-400"
            >
              Select an organization first
            </div>
            <div v-else class="flex flex-wrap gap-2">
              <button
                v-for="category in selectedOrganization?.categories"
                :key="category.categoryId"
                type="button"
                class="px-3.5 py-1.5 rounded-xl text-sm font-medium border-2 transition-all"
                :class="
                  form.categoryId === category.categoryId
                    ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400'
                    : 'border-gray-200 dark:border-gray-700 text-gray-600 dark:text-gray-400 hover:border-gray-300 dark:hover:border-gray-600'
                "
                @click="handleCategorySelect(category.categoryId)"
              >
                {{ category.categoryName }}
              </button>
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
              Priority
            </label>
            <div class="grid grid-cols-4 gap-2">
              <button
                v-for="priority in priorities"
                :key="priority.label"
                type="button"
                class="py-2 rounded-xl text-xs font-medium border-2 transition-all"
                :class="
                  form.priority === priority.value
                    ? `${priorityColors[priority.value]} ring-2 ring-offset-1 dark:ring-offset-gray-900 ring-blue-500/30`
                    : 'border-gray-200 dark:border-gray-700 text-gray-500 dark:text-gray-400 hover:border-gray-300'
                "
                @click="handlePrioritySelect(priority.value)"
              >
                {{ priority.label }}
              </button>
            </div>
          </div>

          <div>
            <label
              for="ticket-subject"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
            >
              Subject <span class="text-red-400">*</span>
            </label>
            <input
              id="ticket-subject"
              v-model="form.subject"
              type="text"
              placeholder="Brief description of your issue..."
              required
              class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all"
            />
          </div>

          <div>
            <label
              for="ticket-message"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
            >
              Message <span class="text-red-400">*</span>
            </label>
            <textarea
              id="ticket-message"
              v-model="form.initialMessage"
              placeholder="Please describe your issue in detail. Include any error messages, steps to reproduce, or relevant context..."
              required
              rows="6"
              maxlength="2000"
              class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all resize-none"
            />
            <p class="text-xs text-gray-400 mt-1">
              {{ form.initialMessage.length }}/2000 characters
            </p>
          </div>
        </div>

        <div class="flex gap-3 mt-4">
          <a
            href="#"
            class="flex-1 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 transition-all text-center"
            @click.prevent="handleCancel"
          >
            Cancel
          </a>
          <button
            type="submit"
            class="flex-1 flex items-center justify-center gap-2 bg-blue-500 hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed text-white py-2.5 rounded-xl text-sm font-medium transition-all shadow-sm"
            :disabled="!canSubmit"
          >
            <Send :size="15" />
            Submit Ticket
          </button>
        </div>
      </form>
    </div>
  </CustomerLayout>
</template>
