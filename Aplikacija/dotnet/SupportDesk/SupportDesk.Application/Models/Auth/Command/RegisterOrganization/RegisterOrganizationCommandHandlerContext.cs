using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Application.Models.Auth.Command.RegisterOrganization;

public record RegisterOrganizationCommandHandlerContext(
    OrganizationName OrganizationName,
    Organization? ExistingOrganization
    ) : ICommandHandlerContext;