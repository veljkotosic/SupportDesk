using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GiveFeedback;

public record GiveFeedbackRequest(Guid TicketId, TicketFeedback Feedback) : IRequest;