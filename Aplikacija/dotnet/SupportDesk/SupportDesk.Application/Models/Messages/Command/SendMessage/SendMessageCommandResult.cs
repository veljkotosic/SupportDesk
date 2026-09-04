using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Messages.Command.SendMessage;

public sealed record SendMessageCommandResult(Guid MessageId) : ICommandResult;