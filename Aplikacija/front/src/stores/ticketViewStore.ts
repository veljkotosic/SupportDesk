import {defineStore} from "pinia";
import {ref} from "vue";
import type {TicketViewInfo} from "@/types/ticket/ticketViewInfo.ts";
import {ticketService} from "@/services/ticket/ticketService.ts";
import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";
import {messageService} from "@/services/message/messageService.ts";
import type {MessageDetails} from "@/types/message/messageDetails.ts";
import type {TicketAssignedInfo} from "@/types/ticket/info/ticketAssignedInfo.ts";
import {TicketStatus} from "@/types/ticket/ticketStatus.ts";
import type {TicketClosedInfo} from "@/types/ticket/info/ticketClosedInfo.ts";

export const useTicketViewStore = defineStore('ticketView', () => {
  const ticket = ref<TicketViewInfo | null>(null)

  async function loadTicket(id: string) {
    try {
      ticket.value = await ticketService.getTicketViewInfo(id)
    } catch (e: any) {

    }
  }

  function unloadTicket() {
    ticket.value = null;
  }

  async function sendMessage(ticketId: string, message: string) {
    try {
      const newMessageId = await messageService.sendMessage(ticketId, message)
    } catch (e: any) {

    }
  }

  function receiveMessage(message: MessageDetails) {
    ticket.value!.messages.push(message)
  }

  async function GiveFeedback(ticketId: string, feedback: TicketFeedback) {
    try {
      await ticketService.giveFeedback(ticketId, feedback)

      ticket.value!.feedback = feedback
    } catch (e: any) {

    }
  }

  function assignTicket(info: TicketAssignedInfo) {
    if (ticket.value!.status !== TicketStatus.Open) {
      return
    }

    ticket.value!.status = TicketStatus.Assigned
    ticket.value!.assignedAt = info.assignedAt
    ticket.value!.supportAgentId = info.supportAgentId
    ticket.value!.supportAgentUsername = info.supportAgentUsername
  }

  function closeTicket(info: TicketClosedInfo) {
    if (ticket.value!.status !== TicketStatus.Assigned) {
      return
    }

    ticket.value!.status = TicketStatus.Closed
    ticket.value!.closedAt = info.closedAt
  }

  async function readNotifications() {
    await ticketService.readAllNotifications(ticket.value!.id)
  }

  return {
    ticket,
    loadTicket,
    unloadTicket,
    sendMessage,
    receiveMessage,
    GiveFeedback,
    assignTicket,
    closeTicket,
    readNotifications
  }
})
