<script setup lang="ts">
import {computed, nextTick, onBeforeMount, onBeforeUnmount, ref, watch} from 'vue'
import {ArrowLeft, Check, Send, ThumbsDown, ThumbsUp, XCircle} from 'lucide-vue-next'
import CustomerLayout from '../../layouts/CustomerDashboardLayout.vue'
import PriorityBadge from '../../components/PriorityBadge.vue'
import StatusBadge from '../../components/StatusBadge.vue'
import CategoryBadge from '../../components/CategoryBadge.vue'
import {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";
import {useRoute} from "vue-router";
import {useAuthStore} from "@/stores/authStore.ts";
import {useTicketViewStore} from "@/stores/ticketViewStore.ts";
import {TicketStatus} from "@/types/ticket/ticketStatus.ts";
import router from "@/router";
import {ticketHubService} from "@/services/hubs/ticketHubService.ts";
import type {MessageDetails} from "@/types/message/messageDetails.ts";
import {storeToRefs} from "pinia";

const route = useRoute()

const authStore = useAuthStore()
const ticketViewStore = useTicketViewStore()

const newMessage = ref('')

const { user } = storeToRefs(authStore)
const ticketId = route.params.id as string

const messagesContainer = ref<HTMLElement | null>(null)

const formatMessageDate = (dateInput: Date | string) => {
  const date = new Date(dateInput)
  const now = new Date()

  const isToday = date.toDateString() === now.toDateString()
  const isCurrentYear = date.getFullYear() === now.getFullYear()

  if (isToday) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false })
  } else if (isCurrentYear) {
    return date.toLocaleString([], {
      hour: '2-digit',
      minute: '2-digit',
      day: 'numeric',
      month: 'short',
      hour12: false
    })
  } else {
    return date.toLocaleString([], {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    })
  }
}

const groupedMessages = computed(() => {
  if (!ticket.value || !ticket.value.messages) return []

  const groups: {
    senderId: string;
    senderUsername: string;
    createdAt: Date;
    messages: MessageDetails[]
  }[] = []

  ticket.value.messages.forEach((message) => {
    const lastGroup = groups[groups.length - 1]
    const messageTime = new Date(message.createdAt).getTime()

    if (
      lastGroup &&
      lastGroup.senderId === message.senderId &&
      messageTime - new Date(lastGroup.messages[lastGroup.messages.length - 1]!.createdAt).getTime() < 5 * 60 * 1000
    ) {
      lastGroup.messages.push(message)
    } else {
      groups.push({
        senderId: message.senderId,
        senderUsername: message.senderUsername,
        createdAt: message.createdAt,
        messages: [message]
      })
    }
  })

  return groups
})

const scrollToBottom = async () => {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

onBeforeMount(async () => {
  await ticketViewStore.loadTicket(ticketId)
  await scrollToBottom()

  await ticketHubService.connect()
  await ticketHubService.joinTicket(ticketId)
  ticketHubService.onNewMessage(async (message) => {
    ticketViewStore.receiveMessage(message)
    await scrollToBottom()
  })
  ticketHubService.onTicketAssigned((info) => {
    ticketViewStore.assignTicket(info)
  })
  ticketHubService.onTicketClosed(async (info) => {
    ticketViewStore.closeTicket(info)
    await scrollToBottom()
  })

  await ticketViewStore.readNotifications()
})

onBeforeUnmount(async () => {
  await ticketViewStore.readNotifications()
  ticketViewStore.unloadTicket()

  ticketHubService.offAll()
  await ticketHubService.leaveTicket(ticketId)
  await ticketHubService.disconnect()
})

watch(() => ticketViewStore.ticket?.messages.length, () => {
  scrollToBottom()
})

function isCustomerMessage(message: MessageDetails): boolean {
  return message.senderId === user.value!.userId
}

const {ticket} = storeToRefs(ticketViewStore)

const isClosed = computed(
  () => ticket!.value!.status === TicketStatus.Closed || ticket!.value!.status === TicketStatus.Archived,
)

const canSendMessage = computed(
  () => newMessage.value.trim().length > 0 && !isClosed.value,
)

function handleBackRoute() {
  router.back()
}

async function handleSendMessage() {
  if (!canSendMessage.value) return

  await ticketViewStore.sendMessage(ticketId, newMessage.value.trim())

  await scrollToBottom()

  newMessage.value = ''
}

async function handleSubmitFeedback(ticketFeedback: TicketFeedback) {
  await ticketViewStore.GiveFeedback(ticketId, ticketFeedback)
}
</script>

<template>
  <CustomerLayout activePath="ticketViewStore.ticket ? `/customer/ticket/${ticketViewStore.ticket.id}` : ''">
    <div v-if="ticketViewStore.ticket" class="flex flex-col h-full">
      <div class="flex items-center gap-3 px-5 py-3.5 bg-white dark:bg-gray-900 border-b border-gray-100 dark:border-gray-800 flex-shrink-0">
        <a
          href="#"
          class="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 transition-all"
          @click.prevent="handleBackRoute"
        >
          <ArrowLeft :size="17" />
        </a>
        <div class="flex-1 min-w-0">
          <h1 class="text-base font-semibold text-gray-900 dark:text-white truncate">
            {{ ticket!.subject }}
          </h1>
          <div class="flex items-center gap-2 mt-0.5 flex-wrap">
            <StatusBadge :status="ticket!.status" />
            <PriorityBadge :priority="ticket!.priority" />
            <CategoryBadge :category="ticket!.categoryName" />
            <span class="text-xs text-gray-400 font-mono">{{ ticket!.id }}</span>
          </div>
        </div>
      </div>

      <div class="px-5 py-3 bg-blue-50 dark:bg-blue-900/10 border-b border-blue-100 dark:border-blue-900/30 flex items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-wrap">
          <span class="text-xs text-blue-600 dark:text-blue-400">
            <span class="text-blue-400 dark:text-blue-500">Organization:</span>
            {{ ticket!.organizationName }}
          </span>
          <span class="text-xs text-blue-600 dark:text-blue-400">
            <span class="text-blue-400 dark:text-blue-500">Opened:</span>
            {{ formatMessageDate(ticket!.openedAt) }}
          </span>
          <span
            v-if="ticket!.supportAgentUsername"
            class="text-xs text-blue-600 dark:text-blue-400"
          >
            <span class="text-blue-400 dark:text-blue-500">Agent:</span>
            {{ ticket!.supportAgentUsername }}
          </span>
          <span
            v-if="!ticket!.supportAgentUsername && !isClosed"
            class="text-xs text-amber-600 dark:text-amber-400 flex items-center gap-1"
          >
            <span class="w-1.5 h-1.5 rounded-full bg-amber-500 animate-pulse" />
            Awaiting agent assignment
          </span>
        </div>
      </div>

      <div
        ref="messagesContainer"
        class="flex-1 overflow-y-auto px-5 py-5 space-y-5 bg-gray-50 dark:bg-gray-950">
        <div
          v-for="(group, groupIndex) in groupedMessages"
          :key="groupIndex"
          class="flex gap-3"
          :class="isCustomerMessage(group.messages[0]!) ? 'flex-row-reverse' : 'flex-row'"
        >
          <div
            class="flex flex-col gap-1 max-w-lg"
            :class="isCustomerMessage(group.messages[0]!) ? 'items-end' : 'items-start'"
          >
            <div
              class="flex items-center gap-2 mb-0.5"
              :class="{ 'flex-row-reverse': isCustomerMessage(group.messages[0]!) }"
            >
              <span class="text-xs font-medium text-gray-600 dark:text-gray-400">
                {{ isCustomerMessage(group.messages[0]!) ? 'You' : group.senderUsername }}
              </span>
              <span
                v-if="!isCustomerMessage(group.messages[0]!)"
                class="text-xs bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 px-1.5 py-0.5 rounded font-medium"
              >
                Support
              </span>
              <span class="text-xs text-gray-400">{{ formatMessageDate(group.createdAt) }}</span>
            </div>
            <div
              v-for="(message, messageIndex) in group.messages"
              :key="message.id"
              class="px-4 py-2.5 text-sm leading-relaxed"
              :class="[
                isCustomerMessage(message)
                  ? 'bg-blue-500 text-white'
                  : 'bg-white dark:bg-gray-900 text-gray-800 dark:text-gray-100 border border-gray-100 dark:border-gray-800 shadow-sm',
                messageIndex === 0
                  ? (isCustomerMessage(message) ? 'rounded-2xl rounded-tr-sm' : 'rounded-2xl rounded-tl-sm')
                  : 'rounded-2xl'
              ]"
            >
              {{ message.text }}
            </div>
          </div>
        </div>

        <template v-if="isClosed">
          <div class="relative my-2">
            <div class="absolute inset-0 flex items-center">
              <div class="w-full border-t border-gray-200 dark:border-gray-700" />
            </div>
            <div class="relative flex justify-center">
              <span class="flex items-center gap-1.5 px-3 py-1 bg-gray-50 dark:bg-gray-950 text-xs text-gray-400 rounded-full">
                <XCircle :size="11" />
                Ticket closed{{ ticket!.closedAt ? ` · ${formatMessageDate(ticket!.closedAt)}` : '' }}
              </span>
            </div>
          </div>

          <div class="flex justify-center">
            <div
              v-if="ticketViewStore.ticket.feedback === TicketFeedback.None"
              class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-5 text-center max-w-xs w-full shadow-sm"
            >
              <p class="text-sm font-medium text-gray-900 dark:text-white mb-1">
                Was this conversation helpful?
              </p>
              <p class="text-xs text-gray-400 mb-4">
                Your feedback helps us improve our support.
              </p>
              <div class="flex items-center justify-center gap-2">
                <button
                  type="button"
                  class="flex items-center gap-1.5 px-3 py-2 rounded-lg border border-emerald-200 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-900/20 text-sm text-emerald-600 dark:text-emerald-400 hover:bg-emerald-100 dark:hover:bg-emerald-900/40 transition-all font-medium"
                  @click="handleSubmitFeedback(TicketFeedback.Helpful)"
                >
                  <ThumbsUp :size="14" />
                  Helpful
                </button>
                <button
                  type="button"
                  class="flex items-center gap-1.5 px-3 py-2 rounded-lg border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/20 text-sm text-red-600 dark:text-red-400 hover:bg-red-100 dark:hover:bg-red-900/40 transition-all font-medium"
                  @click="handleSubmitFeedback(TicketFeedback.Unhelpful)"
                >
                  <ThumbsDown :size="14" />
                  Not Helpful
                </button>
              </div>
            </div>
            <div
              v-else
              class="flex items-center gap-2 px-5 py-3 rounded-xl bg-white dark:bg-gray-900 border border-gray-100 dark:border-gray-800 shadow-sm"
            >
              <Check :size="15" class="text-emerald-500 flex-shrink-0" />
              <span class="text-sm text-gray-600 dark:text-gray-300">
                Thank you for your feedback!
              </span>
            </div>
          </div>
        </template>
      </div>

      <div
        v-if="!isClosed"
        class="flex-shrink-0 p-4 bg-white dark:bg-gray-900 border-t border-gray-100 dark:border-gray-800"
      >
        <div class="flex items-end gap-3 bg-gray-50 dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 px-4 py-3">
          <textarea
            v-model="newMessage"
            placeholder="Type a message..."
            rows="1"
            class="flex-1 bg-transparent text-sm text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 resize-none focus:outline-none leading-relaxed min-h-6 max-h-[120px]"
            @keydown.enter.exact.prevent="handleSendMessage"
          />
          <button
            type="button"
            class="flex-shrink-0 w-8 h-8 rounded-xl flex items-center justify-center transition-all mb-0.5"
            :class="
              canSendMessage
                ? 'bg-blue-500 hover:bg-blue-600 text-white'
                : 'bg-gray-200 dark:bg-gray-700 text-gray-400 cursor-not-allowed'
            "
            :disabled="!canSendMessage"
            @click="handleSendMessage"
          >
            <Send :size="15" />
          </button>
        </div>
        <p class="text-xs text-gray-400 mt-2 px-1">
          Enter to send · Shift+Enter for new line
        </p>
      </div>

      <div
        v-else
        class="flex-shrink-0 px-5 py-3 bg-white dark:bg-gray-900 border-t border-gray-100 dark:border-gray-800"
      >
        <div class="flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700">
          <XCircle :size="14" class="text-gray-400 flex-shrink-0" />
          <span class="text-xs text-gray-400">
            This ticket is closed.
            <RouterLink
              to="/customer/openTicket"
              class="text-blue-500 hover:text-blue-600 font-medium"
            >
              Open a new ticket
            </RouterLink>
            if you need further help.
          </span>
        </div>
      </div>
    </div>
    <div v-else class="flex items-center justify-center h-full">
      <p class="text-gray-500">Loading ticket...</p>
    </div>
  </CustomerLayout>
</template>
