namespace SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo.Dtos;

public sealed record TicketMessageDetailsDto(
    Guid Id,
    Guid SenderId,
    string SenderUserName,
    string Text,
    DateTime CreatedAt);