import {SignalRService} from "@/services/signalR.ts";
import type {Ticket} from "@/types/ticket/ticket.ts";
import type {TicketFeedbackInfo} from "@/types/ticket/info/ticketFeedbackInfo.ts";
import type {TicketAssignedInfo} from "@/types/ticket/info/ticketAssignedInfo.ts";
import type {TicketClosedInfo} from "@/types/ticket/info/ticketClosedInfo.ts";
import type {OrganizationDashboardMessageInfo} from "@/types/organizationAdminDashboard/organizationDashboardMessageInfo.ts";

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

  onTicketAssigned(callback: (ticketAssignedInfo: TicketAssignedInfo) => void): void {
    client.on<TicketAssignedInfo>("TicketAssigned", callback);
  },

  onTicketClosed(callback: (ticketClosedInfo: TicketClosedInfo) => void): void {
    client.on<TicketClosedInfo>("TicketClosed", callback);
  },

  onNewTicketMessage(callback: (message: OrganizationDashboardMessageInfo) => void): void {
    client.on<OrganizationDashboardMessageInfo>("NewTicketMessage", callback);
  },

  offAll(): void {
    client.off("NewTicket");
    client.off("TicketAssigned");
    client.off("TicketClosed");
    client.off("TicketFeedback");
    client.off("NewTicketMessage");
  }
}
