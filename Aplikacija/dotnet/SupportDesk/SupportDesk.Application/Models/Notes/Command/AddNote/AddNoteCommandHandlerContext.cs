using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Notes.Command.AddNote;

internal sealed record AddNoteCommandHandlerContext(
    Ticket? Ticket,
    TicketId TicketId,
    User? User,
    UserId UserId
    ) : ICommandHandlerContext;