using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth;

namespace SupportDesk.Application.Models.Auth.Command.RefreshLogin;

public sealed record RefreshLoginCommandResult(
    AccessToken AccessToken,
    RefreshToken RefreshToken
    ) : ICommandResult;