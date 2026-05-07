using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterOrganization;

public record RegisterOrganizationRequest(
    string Username,
    string OrganizationName,
    string Email,
    string Password
    ) : IRequest<RegisterOrganizationResult>;