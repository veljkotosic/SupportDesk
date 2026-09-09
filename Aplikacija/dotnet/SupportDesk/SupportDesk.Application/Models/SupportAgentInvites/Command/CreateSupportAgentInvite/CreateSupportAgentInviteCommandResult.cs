using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.SupportAgentInvites.Command.CreateSupportAgentInvite;

public sealed record CreateSupportAgentInviteCommandResult(string Code) : ICommandResult;