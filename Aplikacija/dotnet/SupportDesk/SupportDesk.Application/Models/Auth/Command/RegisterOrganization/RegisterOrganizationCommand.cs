using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Auth.Command.RegisterOrganization;

public record RegisterOrganizationCommand(
    string Username,
    string OrganizationName,
    string Email,
    string Password
    ) : ICommand<RegisterOrganizationCommandResult>;