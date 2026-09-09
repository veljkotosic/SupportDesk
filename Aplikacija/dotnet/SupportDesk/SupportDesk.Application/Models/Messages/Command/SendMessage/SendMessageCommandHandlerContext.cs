using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.Application.Models.Messages.Command.SendMessage;

internal sealed record SendMessageCommandHandlerContext(
    Ticket? Ticket,
    TicketId TicketId,
    User User
    ) : ICommandHandlerContext;