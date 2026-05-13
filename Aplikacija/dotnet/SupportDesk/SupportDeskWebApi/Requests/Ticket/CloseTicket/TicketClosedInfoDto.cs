namespace SupportDeskWebApi.Requests.Ticket.CloseTicket;

public record TicketClosedInfoDto(
    Guid TicketId,
    DateTime ClosedAt);