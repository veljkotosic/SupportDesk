using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Events;

public sealed record TicketFeedbackGivenDomainEvent(
    TicketId TicketId,
    OrganizationId OrganizationId,
    TicketFeedback Feedback) : IDomainEvent;