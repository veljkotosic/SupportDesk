namespace SupportDeskWebApi.Requests.Ticket.CloseTicket;

public record TicketClosedInfoDto(
    Guid TicketId,
    Guid SupportAgentId,
    DateTime ClosedAt);
