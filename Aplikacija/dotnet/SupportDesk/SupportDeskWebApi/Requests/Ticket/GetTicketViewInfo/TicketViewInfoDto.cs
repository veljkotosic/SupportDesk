using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Requests.Message.Common;
using SupportDeskWebApi.Requests.Note.Common;

namespace SupportDeskWebApi.Requests.Ticket.GetTicketViewInfo;

public record TicketViewInfoDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid CategoryId,
    string CategoryName,
    Guid CustomerId,
    string CustomerUsername,
    Guid? SupportAgentId,
    string? SupportAgentUsername,
    TicketStatus Status,
    TicketPriority Priority,
    string Subject,
    DateTime OpenedAt,
    DateTime? AssignedAt,
    DateTime? ClosedAt,
    TicketFeedback Feedback,
    List<MessageDetailsDto> Messages,
    List<NoteDetailsDto> Notes);