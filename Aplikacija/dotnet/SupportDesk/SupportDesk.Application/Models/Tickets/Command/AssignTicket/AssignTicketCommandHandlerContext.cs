using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Command.AssignTicket;

internal sealed record AssignTicketCommandHandlerContext(
    Ticket? Ticket,
    TicketId TicketId
    ) : ICommandHandlerContext;