namespace SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo.Dtos;

public sealed record TicketNoteDetailsDto(
    Guid Id,
    Guid AuthorId,
    string AuthorUserName,
    string Text,
    DateTime CreatedAt);