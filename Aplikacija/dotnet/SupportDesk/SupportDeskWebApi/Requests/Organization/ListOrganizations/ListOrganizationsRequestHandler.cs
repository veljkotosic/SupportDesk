using SupportDeskWebApi.Data.Entities.Organization.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListOrganizations;

public class ListOrganizationsRequestHandler
    : IRequestHandler<ListOrganizationsRequest, ListOrganizationsResult>
{
    private readonly IOrganizationRepository _organizationRepository;

    public ListOrganizationsRequestHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<ListOrganizationsResult> HandleAsync(ListOrganizationsRequest request, CancellationToken cancellationToken = default)
    {
        var organizationListings = await _organizationRepository.ListAllOrganizations(cancellationToken);
        
        return new ListOrganizationsResult(organizationListings);
    }
}