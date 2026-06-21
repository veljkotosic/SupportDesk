namespace SupportDeskWebApi.Requests.Message.Common;

public record MessageDetailsDto(
    Guid Id,
    Guid SenderId,
    string SenderUsername,
    string Text,
    DateTime CreatedAt);