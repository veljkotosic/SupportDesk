using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;
using SupportDeskWebApi.Requests.Organization.ListOrganizations;

namespace SupportDeskWebApi.Data.Entities.Organization.Repository;

public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }

    public async Task<Organization?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Context.Organizations.FirstOrDefaultAsync(o => o.Name == name, cancellationToken);
    }

    public async Task<List<OrganizationListingDto>> ListAllOrganizations(CancellationToken cancellationToken = default)
    {
        return await Context.Organizations
            .Select(org => new OrganizationListingDto(org.Id, org.Name))
            .ToListAsync(cancellationToken);
    }
}