using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth;

namespace SupportDesk.Application.Models.Auth.Command.RegisterOrganization;

public sealed record RegisterOrganizationCommandResult(
    AccessToken AccessToken,
    RefreshToken RefreshToken
    ) :  ICommandResult;