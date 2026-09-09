using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Auth.Command.RegisterSupportAgent;

public sealed record RegisterSupportAgentCommand(
    string UserName,
    string Email,
    string Password,
    Guid Code
    ) : ICommand<RegisterSupportAgentCommandResult>;