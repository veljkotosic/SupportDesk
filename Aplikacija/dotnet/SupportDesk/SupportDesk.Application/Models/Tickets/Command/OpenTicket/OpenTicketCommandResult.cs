using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Tickets.Command.OpenTicket;

public sealed record OpenTicketCommandResult(Guid TicketId) : ICommandResult;