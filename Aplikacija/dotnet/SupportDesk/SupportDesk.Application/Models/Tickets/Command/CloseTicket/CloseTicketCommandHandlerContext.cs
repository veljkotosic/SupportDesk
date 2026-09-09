using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Command.CloseTicket;

internal sealed record CloseTicketCommandHandlerContext(
    Ticket? Ticket,
    TicketId TicketId,
    UserId UserId
    ) : ICommandHandlerContext;