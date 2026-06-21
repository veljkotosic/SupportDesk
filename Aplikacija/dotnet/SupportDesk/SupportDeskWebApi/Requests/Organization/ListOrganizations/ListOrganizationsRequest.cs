using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListOrganizations;

public record ListOrganizationsRequest() : IRequest<ListOrganizationsResult>;