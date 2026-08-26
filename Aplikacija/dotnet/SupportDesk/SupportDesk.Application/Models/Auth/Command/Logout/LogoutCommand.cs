using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Auth.Command.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;