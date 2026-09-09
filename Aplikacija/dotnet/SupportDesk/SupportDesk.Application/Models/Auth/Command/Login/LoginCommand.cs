using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Auth.Command.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginCommandResult>;