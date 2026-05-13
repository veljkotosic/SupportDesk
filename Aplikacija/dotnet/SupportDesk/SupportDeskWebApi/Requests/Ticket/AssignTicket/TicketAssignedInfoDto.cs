namespace SupportDeskWebApi.Requests.Ticket.AssignTicket;

public record TicketAssignedInfoDto(
    Guid TicketId,
    string Username,
    string Email,
    DateTime AssignedAt);