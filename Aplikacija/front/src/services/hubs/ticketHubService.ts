import {SignalRService} from "@/services/signalR.ts";
import type {Message} from "@/types/message/message.ts";
import type {Note} from "@/types/note/note.ts";
import type {TicketAssignedInfo} from "@/types/ticket/info/ticketAssignedInfo.ts";
import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";
import type {TicketClosedInfo} from "@/types/ticket/info/ticketClosedInfo.ts";
import type {TicketFeedbackInfo} from "@/types/ticket/info/ticketFeedbackInfo.ts";

const client = new SignalRService(`/hubs/ticketHub`);

export const ticketHubService = {
  async connect(): Promise<void> {
    await client.start();
  },

  async disconnect(): Promise<void> {
    await client.stop();
  },

  async joinTicket(ticketId: string): Promise<void> {
    await client.invoke("JoinTicket", ticketId);
  },

  async leaveTicket(ticketId: string): Promise<void> {
    await client.invoke("LeaveTicket", ticketId);
  },

  onNewMessage(callback: (message: Message) => void): void {
    client.on<Message>("NewMessage", callback);
  },

  onNewNote(callback: (note: Note) => void): void {
    client.on<Note>("NewNote", callback);
  },

  onTicketAssigned(callback: (ticketAssignedInfo: TicketAssignedInfo) => void): void {
    client.on<TicketAssignedInfo>("TicketAssigned", callback);
  },

  onTicketClosed(callback: (ticketClosedInfo: TicketClosedInfo) => void): void {
    client.on<TicketClosedInfo>("TicketClosed", callback);
  },

  onGivenFeedback(callback: (ticketFeedbackInfo: TicketFeedbackInfo) => void): void {
    client.on<TicketFeedbackInfo>("TicketFeedback", callback);
  },

  offAll(): void {
    client.off("NewMessage");
    client.off("NewNote");
    client.off("TicketAssigned");
    client.off("TicketClosed");
    client.off("TicketFeedback");
  }
}
