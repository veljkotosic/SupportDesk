using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Auth.Command.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    string UserName,
    string Email,
    string Password
    ) : ICommand<RegisterCustomerCommandResult>;