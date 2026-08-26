using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth;

namespace SupportDesk.Application.Models.Auth.Command.RegisterSupportAgent;

public sealed record RegisterSupportAgentCommandResult(
    AccessToken AccessToken,
    RefreshToken RefreshToken
    ) : ICommandResult;