<script setup lang="ts">
import { computed, nextTick, onBeforeMount, onBeforeUnmount, ref } from 'vue'
import {
  ArrowLeft, Building2, Calendar, Clock, FileText, Lock, Plus, Send, Tag, User, UserCheck, X, XCircle,
} from 'lucide-vue-next'
import { storeToRefs } from 'pinia'
import CategoryBadge from '@/components/CategoryBadge.vue'
import PriorityBadge from '@/components/PriorityBadge.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import UserAvatar from '@/components/UserAvatar.vue'
import ErrorBanner from '@/components/ErrorBanner.vue'
import { useAuthStore } from '@/stores/authStore.ts'
import { useTicketViewStore } from '@/stores/ticketViewStore.ts'
import { ticketHubService } from '@/services/hubs/ticketHubService.ts'
import { TicketFeedback } from '@/types/ticket/ticketFeedback.ts'
import { TicketStatus } from '@/types/ticket/ticketStatus.ts'
import { UserType } from '@/types/user/userType.ts'
import type { MessageDetails } from '@/types/message/messageDetails.ts'
import { isRealDate } from '@/utility/date.ts'
import { templateAnswerService } from '@/services/templateAnswer/templateAnswerService.ts'
import type { TemplateAnswer } from '@/types/templateAnswer/templateAnswer.ts'

const props = defineProps<{
  ticketId: string
  backPath: string
}>()

const authStore = useAuthStore()
const ticketStore = useTicketViewStore()
const { ticket } = storeToRefs(ticketStore)
const newMessage = ref('')
const newNote = ref('')
const showNoteForm = ref(false)
const showTemplates = ref(false)
const templateAnswers = ref<TemplateAnswer[]>([])
const errorMessage = ref('')
const messagesContainer = ref<HTMLElement | null>(null)
const messageInput = ref<HTMLTextAreaElement | null>(null)

const isAdmin = computed(() => authStore.user?.type === UserType.OrganizationAdmin)
const isSupportAgent = computed(() => authStore.user?.type === UserType.SupportAgent)
const isClosed = computed(() => ticket.value?.status === TicketStatus.Closed)
const isBlockedAgent = computed(() =>
  isSupportAgent.value &&
  ticket.value?.status === TicketStatus.Assigned &&
  ticket.value.supportAgentId !== authStore.user?.userId,
)
const canClose = computed(() =>
  isSupportAgent.value &&
  ticket.value?.status === TicketStatus.Assigned &&
  ticket.value.supportAgentId === authStore.user?.userId,
)
const canSendMessage = computed(() => canClose.value && newMessage.value.trim().length > 0)

const groupedMessages = computed(() => {
  const groups: { senderId: string; senderUsername: string; createdAt: Date | string; messages: MessageDetails[] }[] = []
  for (const message of ticket.value?.messages ?? []) {
    const lastGroup = groups.at(-1)
    if (
      lastGroup &&
      lastGroup.senderId === message.senderId &&
      new Date(message.createdAt).getTime() - new Date(lastGroup.messages.at(-1)!.createdAt).getTime() < 5 * 60 * 1000
    ) {
      lastGroup.messages.push(message)
    } else {
      groups.push({
        senderId: message.senderId,
        senderUsername: message.senderUsername,
        createdAt: message.createdAt,
        messages: [message],
      })
    }
  }
  return groups
})

onBeforeMount(async () => {
  await ticketStore.loadTicket(props.ticketId)

  if (isSupportAgent.value && ticket.value?.status === TicketStatus.Open) {
    await ticketStore.assignCurrentTicket(props.ticketId)
    await ticketStore.reloadTicket()
  }

  if (isSupportAgent.value) {
    await loadTemplateAnswers()
  }

  await ticketHubService.connect()
  await ticketHubService.joinTicket(props.ticketId)
  ticketHubService.onNewMessage(async message => {
    ticketStore.receiveMessage(message)
    await scrollToBottom()
  })
  ticketHubService.onNewNote(ticketStore.reloadTicket)
  ticketHubService.onTicketAssigned(ticketStore.assignTicket)
  ticketHubService.onTicketClosed(ticketStore.closeTicket)
  ticketHubService.onGivenFeedback(info => {
    if (ticket.value) ticket.value.feedback = info.feedback
  })
  await scrollToBottom()
})

onBeforeUnmount(async () => {
  ticketHubService.offAll()
  await ticketHubService.leaveTicket(props.ticketId)
  await ticketHubService.disconnect()
  ticketStore.unloadTicket()
})

async function scrollToBottom() {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

async function loadTemplateAnswers() {
  try {
    const result = await templateAnswerService.getTemplateAnswers()
    templateAnswers.value = result.templateAnswers
  } catch (e: any) {
    templateAnswers.value = []
  }
}

function formatDate(dateInput?: Date | string) {
  if (!isRealDate(dateInput)) return '—'
  return new Date(dateInput).toLocaleString([], {
    day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: false,
  })
}

function isCustomerMessage(message: MessageDetails) {
  return message.senderId === ticket.value?.customerId
}

async function handleSendMessage() {
  if (!canSendMessage.value) return
  errorMessage.value = ''
  try {
    await ticketStore.sendMessage(props.ticketId, newMessage.value.trim())
    newMessage.value = ''
    showTemplates.value = false
  } catch (e: any) {
    errorMessage.value = e?.message ?? 'Could not send message.'
  }
}

async function handleAddNote() {
  const text = newNote.value.trim()
  if (!text) return
  errorMessage.value = ''
  try {
    await ticketStore.addNote(props.ticketId, text)
    newNote.value = ''
    showNoteForm.value = false
  } catch (e: any) {
    errorMessage.value = e?.message ?? 'Could not save note.'
  }
}

async function handleCloseTicket() {
  if (canClose.value) {
    errorMessage.value = ''
    try {
      await ticketStore.closeCurrentTicket(props.ticketId)
    } catch (e: any) {
      errorMessage.value = e?.message ?? 'Could not close ticket.'
    }
  }
}

function handleToggleTemplates() {
  showTemplates.value = !showTemplates.value
}

function handleCloseTemplates() {
  showTemplates.value = false
}

async function handleUseTemplate(template: TemplateAnswer) {
  newMessage.value = template.text
  showTemplates.value = false
  await nextTick()
  messageInput.value?.focus()
}
</script>

<template>
  <div v-if="ticket" class="flex flex-col h-full">
    <div class="flex items-center gap-3 px-5 py-3.5 bg-white dark:bg-gray-900 border-b border-gray-100 dark:border-gray-800 flex-shrink-0">
      <RouterLink :to="backPath" class="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 transition-all">
        <ArrowLeft :size="17" />
      </RouterLink>
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2 flex-wrap">
          <h1 class="text-base font-semibold text-gray-900 dark:text-white truncate">{{ ticket.subject }}</h1>
          <span class="text-xs text-gray-400 font-mono">{{ ticket.id }}</span>
        </div>
        <div class="flex items-center gap-2 mt-1">
          <StatusBadge :status="ticket.status" />
          <PriorityBadge :priority="ticket.priority" />
          <CategoryBadge :category="ticket.categoryName" />
        </div>
      </div>
      <button
        v-if="canClose"
        type="button"
        class="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-red-500 hover:bg-red-600 rounded-lg transition-all"
        @click="handleCloseTicket"
      >
        <XCircle :size="14" />
        Close Ticket
      </button>
      <span v-if="isAdmin" class="hidden sm:flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-indigo-50 dark:bg-indigo-900/20 text-indigo-600 dark:text-indigo-400 border border-indigo-200 dark:border-indigo-800">
        View only
      </span>
    </div>

    <div v-if="isBlockedAgent" class="flex-1 flex flex-col items-center justify-center gap-4 p-8 text-center">
      <div class="w-14 h-14 rounded-2xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
        <Lock :size="24" class="text-gray-400" />
      </div>
      <div>
        <h2 class="font-semibold text-gray-900 dark:text-white mb-1">Ticket already assigned</h2>
        <p class="text-sm text-gray-500 dark:text-gray-400 max-w-sm">
          This ticket is currently being handled by {{ ticket.supportAgentUsername }}.
        </p>
      </div>
      <RouterLink :to="backPath" class="px-4 py-2 rounded-xl bg-blue-500 hover:bg-blue-600 text-white text-sm font-medium">
        Back to Tickets
      </RouterLink>
    </div>

    <div v-else class="flex flex-1 min-h-0 overflow-hidden">
      <aside class="hidden md:flex w-72 xl:w-80 flex-shrink-0 flex-col bg-white dark:bg-gray-900 border-r border-gray-100 dark:border-gray-800 overflow-y-auto">
        <div class="p-4 border-b border-gray-100 dark:border-gray-800">
          <p class="text-xs font-medium text-gray-400 uppercase tracking-wide mb-3">Ticket Info</p>
          <div class="space-y-3">
            <div class="flex items-start gap-2.5">
              <User :size="14" class="text-gray-400 mt-0.5" />
              <div>
                <p class="text-xs text-gray-400">Customer</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white">{{ ticket.customerUsername }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">{{ ticket.customerEmail }}</p>
              </div>
            </div>
            <div class="flex items-start gap-2.5"><Building2 :size="14" class="text-gray-400 mt-0.5" /><div><p class="text-xs text-gray-400">Organization</p><p class="text-sm text-gray-700 dark:text-gray-300">{{ ticket.organizationName }}</p></div></div>
            <div class="flex items-start gap-2.5"><Tag :size="14" class="text-gray-400 mt-0.5" /><div><p class="text-xs text-gray-400">Category</p><p class="text-sm text-gray-700 dark:text-gray-300">{{ ticket.categoryName }}</p></div></div>
            <div class="flex items-start gap-2.5"><Calendar :size="14" class="text-gray-400 mt-0.5" /><div><p class="text-xs text-gray-400">Opened</p><p class="text-sm text-gray-700 dark:text-gray-300">{{ formatDate(ticket.openedAt) }}</p></div></div>
            <div class="flex items-start gap-2.5"><UserCheck :size="14" class="text-gray-400 mt-0.5" /><div><p class="text-xs text-gray-400">Assigned</p><p class="text-sm text-gray-700 dark:text-gray-300">{{ formatDate(ticket.assignedAt) }}</p></div></div>
            <div class="flex items-start gap-2.5"><XCircle :size="14" class="text-gray-400 mt-0.5" /><div><p class="text-xs text-gray-400">Closed</p><p class="text-sm text-gray-700 dark:text-gray-300">{{ formatDate(ticket.closedAt) }}</p></div></div>
            <div class="flex items-start gap-2.5"><Clock :size="14" class="text-gray-400 mt-0.5" /><div><p class="text-xs text-gray-400">Last Activity</p><p class="text-sm text-gray-700 dark:text-gray-300">{{ formatDate(ticket.lastMessageAt) }}</p></div></div>
          </div>
        </div>

        <div class="p-4 border-b border-gray-100 dark:border-gray-800">
          <p class="text-xs font-medium text-gray-400 uppercase tracking-wide mb-3">Assigned Agent</p>
          <div v-if="ticket.supportAgentUsername" class="flex items-center gap-2.5">
            <UserAvatar :user-name="ticket.supportAgentUsername" />
            <p class="text-sm font-medium text-gray-900 dark:text-white">{{ ticket.supportAgentUsername }}</p>
          </div>
          <p v-else class="text-sm text-gray-400">Not assigned yet</p>
        </div>

        <div class="p-4">
          <div class="flex items-center justify-between mb-3">
            <p class="text-xs font-medium text-gray-400 uppercase tracking-wide">Internal Notes</p>
            <button v-if="!isClosed" type="button" class="flex items-center gap-1 text-xs text-blue-500 hover:text-blue-600 font-medium" @click="showNoteForm = !showNoteForm">
              <Plus :size="13" /> Add note
            </button>
          </div>
          <div v-if="showNoteForm" class="mb-4 p-3 rounded-xl bg-amber-50 dark:bg-amber-900/10 border border-amber-200 dark:border-amber-800">
            <textarea v-model="newNote" rows="3" placeholder="Add an internal note..." class="w-full bg-transparent text-sm text-gray-900 dark:text-white resize-none focus:outline-none" />
            <div class="flex justify-end gap-2 mt-2">
              <button type="button" class="text-xs text-gray-500 px-2 py-1" @click="showNoteForm = false">Cancel</button>
              <button type="button" class="text-xs bg-amber-400 text-amber-900 px-3 py-1 rounded-lg font-medium" @click="handleAddNote">Save note</button>
            </div>
          </div>
          <p v-if="ticket.notes.length === 0" class="text-xs text-gray-400 text-center py-4">No internal notes yet</p>
          <div v-for="note in ticket.notes" :key="note.id" class="mb-3 p-3 rounded-xl bg-amber-50 dark:bg-amber-900/10 border border-amber-100 dark:border-amber-900/30">
            <div class="flex items-center justify-between mb-1.5">
              <span class="text-xs font-medium text-amber-700 dark:text-amber-400">{{ note.authorUsername }}</span>
              <span class="text-xs text-amber-500/70">{{ formatDate(note.createdAt) }}</span>
            </div>
            <p class="text-xs text-amber-800 dark:text-amber-300 leading-relaxed">{{ note.text }}</p>
          </div>
        </div>
      </aside>

      <section class="flex-1 flex flex-col bg-gray-50 dark:bg-gray-950 min-w-0">
        <ErrorBanner class="mx-5 mt-4" :message="errorMessage" dismissible @dismiss="errorMessage = ''" />

        <div ref="messagesContainer" class="flex-1 overflow-y-auto px-5 py-5 space-y-5">
          <div v-for="group in groupedMessages" :key="`${group.senderId}-${group.createdAt}`" class="flex gap-3" :class="isCustomerMessage(group.messages[0]!) ? 'flex-row-reverse' : 'flex-row'">
            <UserAvatar :user-name="group.senderUsername" />
            <div class="flex flex-col gap-1 max-w-lg" :class="isCustomerMessage(group.messages[0]!) ? 'items-end' : 'items-start'">
              <div class="flex items-center gap-2">
                <span class="text-xs font-medium text-gray-600 dark:text-gray-400">{{ group.senderUsername }}</span>
                <span class="text-xs text-gray-400">{{ formatDate(group.createdAt) }}</span>
              </div>
              <div
                v-for="message in group.messages"
                :key="message.id"
                class="px-4 py-2.5 rounded-2xl text-sm leading-relaxed"
                :class="isCustomerMessage(message) ? 'bg-blue-500 text-white rounded-tr-sm' : 'bg-white dark:bg-gray-900 text-gray-800 dark:text-gray-100 border border-gray-100 dark:border-gray-800 rounded-tl-sm shadow-sm'"
              >
                {{ message.text }}
              </div>
            </div>
          </div>

          <div v-if="isClosed" class="relative my-2">
            <div class="absolute inset-0 flex items-center"><div class="w-full border-t border-gray-200 dark:border-gray-700" /></div>
            <div class="relative flex justify-center">
              <span class="flex items-center gap-1.5 px-3 py-1 bg-gray-50 dark:bg-gray-950 text-xs text-gray-400 rounded-full">
                <XCircle :size="11" />
                Ticket closed{{ isRealDate(ticket.closedAt) ? ` · ${formatDate(ticket.closedAt)}` : '' }}
              </span>
            </div>
          </div>
          <p v-if="isClosed" class="text-center text-xs text-gray-400">
            {{ ticket.feedback === TicketFeedback.None ? 'Awaiting customer feedback.' : ticket.feedback === TicketFeedback.Helpful ? 'Customer found this conversation helpful.' : 'Customer found this conversation unhelpful.' }}
          </p>
        </div>

        <div v-if="isAdmin || isClosed" class="flex-shrink-0 px-5 py-3 bg-white dark:bg-gray-900 border-t border-gray-100 dark:border-gray-800">
          <div class="flex items-center justify-center px-4 py-2.5 rounded-xl bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700">
            <span class="text-xs text-gray-400">{{ isClosed ? 'This ticket is closed.' : 'Organization admins can view conversations but cannot send messages.' }}</span>
          </div>
        </div>
        <div v-else class="flex-shrink-0 p-4 bg-white dark:bg-gray-900 border-t border-gray-100 dark:border-gray-800">
          <div
            v-if="showTemplates"
            class="mb-3 bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-lg overflow-hidden"
          >
            <div class="flex items-center justify-between px-3 py-2 border-b border-gray-100 dark:border-gray-800">
              <span class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide">Template Answers</span>
              <button
                type="button"
                class="p-0.5 rounded text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                @click="handleCloseTemplates"
              >
                <X :size="13" />
              </button>
            </div>
            <div v-if="templateAnswers.length === 0" class="px-3 py-5 text-center text-xs text-gray-400">
              No template answers available.
            </div>
            <div v-else class="max-h-64 overflow-y-auto">
              <button
                v-for="template in templateAnswers"
                :key="template.id"
                type="button"
                class="w-full text-left px-3 py-2.5 hover:bg-gray-50 dark:hover:bg-gray-800 border-b border-gray-50 dark:border-gray-800 last:border-b-0 transition-colors"
                @click="handleUseTemplate(template)"
              >
                <p class="text-sm font-medium text-gray-900 dark:text-white">{{ template.title }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400 line-clamp-2 mt-0.5">{{ template.text }}</p>
              </button>
            </div>
          </div>
          <div class="flex items-end gap-3 bg-gray-50 dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 px-4 py-3">
            <button
              type="button"
              class="flex-shrink-0 mb-0.5 transition-colors"
              :class="showTemplates ? 'text-blue-500' : 'text-gray-400 hover:text-gray-600 dark:hover:text-gray-300'"
              title="Template answers"
              @click="handleToggleTemplates"
            >
              <FileText :size="18" />
            </button>
            <textarea ref="messageInput" v-model="newMessage" rows="1" placeholder="Type a reply..." class="flex-1 bg-transparent text-sm text-gray-900 dark:text-white resize-none focus:outline-none min-h-6 max-h-[120px]" @keydown.enter.exact.prevent="handleSendMessage" />
            <button type="button" class="w-8 h-8 rounded-xl flex items-center justify-center" :class="canSendMessage ? 'bg-blue-500 hover:bg-blue-600 text-white' : 'bg-gray-200 dark:bg-gray-700 text-gray-400 cursor-not-allowed'" :disabled="!canSendMessage" @click="handleSendMessage">
              <Send :size="15" />
            </button>
          </div>
        </div>
      </section>
    </div>
  </div>
  <div v-else class="h-full flex items-center justify-center text-sm text-gray-500">Loading ticket...</div>
</template>
