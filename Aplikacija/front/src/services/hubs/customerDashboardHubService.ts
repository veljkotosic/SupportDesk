import {SignalRService} from "@/services/signalR.ts";
import type {TicketAssignedInfo} from "@/types/ticket/info/ticketAssignedInfo.ts";
import type {TicketClosedInfo} from "@/types/ticket/info/ticketClosedInfo.ts";
import type {Message} from "@/types/message/message.ts";

const client = new SignalRService(`/hubs/customerDashboardHub`);

export const customerDashboardHubService = {
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

  onTicketAssigned(callback: (ticketAssignedInfo: TicketAssignedInfo) => void): void {
    client.on<TicketAssignedInfo>("TicketAssigned", callback);
  },

  onTicketClosed(callback: (ticketClosedInfo: TicketClosedInfo) => void): void {
    client.on<TicketClosedInfo>("TicketClosed", callback);
  },

  onNewTicketMessage(callback: (message: Message) => void): void {
    client.on<Message>("NewTicketMessage", callback);
  },

  offAll(): void {
    client.off("TicketAssigned");
    client.off("TicketClosed");
    client.off("NewTicketMessage");
  }
}
