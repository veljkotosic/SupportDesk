using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth;

namespace SupportDesk.Application.Models.Auth.Command.Logout;

internal sealed record LogoutCommandHandlerContext(Guid UserId, RefreshToken RefreshToken) : ICommandHandlerContext;