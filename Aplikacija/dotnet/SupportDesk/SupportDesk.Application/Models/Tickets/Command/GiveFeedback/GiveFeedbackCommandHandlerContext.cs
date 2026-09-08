using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Command.GiveFeedback;

internal sealed record GiveFeedbackCommandHandlerContext(
    Ticket? Ticket,
    TicketId TicketId
    ) : ICommandHandlerContext;