using SupportDeskWebApi.Data.Entities.Common.Repository;
using SupportDeskWebApi.Requests.Organization.ListOrganizations;

namespace SupportDeskWebApi.Data.Entities.Organization.Repository;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<OrganizationListingDto>> ListAllOrganizations(CancellationToken cancellationToken = default);
}