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
import {noteService} from "@/services/note/noteService.ts";
import type {NoteDetails} from "@/types/note/noteDetails.ts";

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
    await messageService.sendMessage(ticketId, message)
  }

  async function addNote(ticketId: string, text: string) {
    await noteService.addNote(ticketId, text)
    await reloadTicket()
  }

  async function assignCurrentTicket(ticketId: string) {
    await ticketService.assignTicket(ticketId)
  }

  async function closeCurrentTicket(ticketId: string) {
    await ticketService.closeTicket(ticketId)
  }

  async function reloadTicket() {
    if (ticket.value) {
      await loadTicket(ticket.value.id)
    }
  }

  function receiveNote(note: NoteDetails) {
    ticket.value!.notes.push(note)
  }

  function receiveMessage(message: MessageDetails) {
    ticket.value!.messages.push(message)
  }

  async function GiveFeedback(ticketId: string, feedback: TicketFeedback) {
    await ticketService.giveFeedback(ticketId, feedback)
    ticket.value!.feedback = feedback
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
    addNote,
    assignCurrentTicket,
    closeCurrentTicket,
    reloadTicket,
    receiveMessage,
    receiveNote,
    GiveFeedback,
    assignTicket,
    closeTicket,
    readNotifications
  }
})
