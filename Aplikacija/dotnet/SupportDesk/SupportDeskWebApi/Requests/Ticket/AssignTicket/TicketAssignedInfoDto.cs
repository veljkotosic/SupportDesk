namespace SupportDeskWebApi.Requests.Ticket.AssignTicket;

public record TicketAssignedInfoDto(
    Guid TicketId,
    Guid SupportAgentId,
    string SupportAgentUsername,
    DateTime AssignedAt);