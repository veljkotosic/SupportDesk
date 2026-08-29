using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Auth.Command.RefreshLogin;

public sealed record RefreshLoginCommand(string RefreshToken) : ICommand<RefreshLoginCommandResult>;