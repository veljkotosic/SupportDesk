namespace SupportDeskWebApi.Requests.Message.SendMessage;

public record SendMessageDto(
    Guid Id,
    Guid OrganizationId,
    Guid TicketId,
    Guid SenderId,
    string Text,
    DateTime CreatedAt);