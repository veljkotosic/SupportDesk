using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.WebApi.Controllers.v1.Ticket.Requests;

public sealed record GiveFeedbackRequest(TicketFeedback Feedback);