import {SignalRService} from "@/services/signalR.ts";
import type {Ticket} from "@/types/ticket/ticket.ts";
import type {TicketFeedback} from "@/types/ticket/ticketFeedback.ts";
import type {TicketFeedbackInfo} from "@/types/ticket/info/ticketFeedbackInfo.ts";

const client = new SignalRService(`/hubs/organizationDashboardHub`);

export const organizationDashboardHubService = {
  async connect(): Promise<void> {
    await client.start();
  },

  async disconnect(): Promise<void> {
    await client.stop();
  },

  async startLiveUpdates(): Promise<void> {
    await client.invoke("StartLiveUpdates");
  },

  async stopLiveUpdates(): Promise<void> {
    await client.invoke("StopLiveUpdates");
  },

  onNewTicket(callback: (ticket: Ticket) => void): void {
    client.on<Ticket>("NewTicket", callback);
  },

  onGivenFeedback(callback: (ticketFeedbackInfo: TicketFeedbackInfo) => void): void {
    client.on<TicketFeedbackInfo>("TicketFeedback", callback);
  },

  offAll(): void {
    client.off("NewTicket");
    client.off("TicketFeedback");
  }
}
