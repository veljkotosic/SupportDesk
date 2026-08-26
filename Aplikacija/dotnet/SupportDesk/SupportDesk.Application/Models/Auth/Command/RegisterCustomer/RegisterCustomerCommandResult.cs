using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth;

namespace SupportDesk.Application.Models.Auth.Command.RegisterCustomer;

public sealed record RegisterCustomerCommandResult(
    AccessToken AccessToken,
    RefreshToken RefreshToken
    ) : ICommandResult;