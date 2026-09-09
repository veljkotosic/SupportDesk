using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Events;

public sealed record TicketAssignedDomainEvent(
    TicketId TicketId,
    OrganizationId OrganizationId,
    UserId SupportAgentId,
    UserId CustomerId
    ) : IDomainEvent;