using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListOrganizations;

public record ListOrganizationsResult(List<OrganizationListingDto> Organizations) : IRequestResult;