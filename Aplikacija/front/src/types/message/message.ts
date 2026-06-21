export interface Message {
  id: string;
  organizationId: string;
  ticketId: string,
  senderId: string,
  text: string,
  createdAt: Date
}
