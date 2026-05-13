using SupportDeskWebApi.Data.Entities.Ticket.Enums;

namespace SupportDeskWebApi.Requests.Ticket.GiveFeedback;

public record TicketFeedbackInfoDto(
    Guid TicketId,
    TicketFeedback Feedback);