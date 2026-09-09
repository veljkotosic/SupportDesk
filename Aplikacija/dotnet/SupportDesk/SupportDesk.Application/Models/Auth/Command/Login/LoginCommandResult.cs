using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth;

namespace SupportDesk.Application.Models.Auth.Command.Login;

public sealed record LoginCommandResult(AccessToken AccessToken, RefreshToken RefreshToken) : ICommandResult;