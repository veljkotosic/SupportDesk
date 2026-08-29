using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth;

namespace SupportDesk.Application.Models.Auth.Command.RefreshLogin;

internal sealed record RefreshLoginCommandHandlerContext(
    Guid UserId,
    RefreshToken RefreshToken
    ) : ICommandHandlerContext;