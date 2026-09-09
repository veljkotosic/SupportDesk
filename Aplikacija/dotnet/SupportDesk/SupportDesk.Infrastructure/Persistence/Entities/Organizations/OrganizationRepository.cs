using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.Repository;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Organizations;

public sealed class OrganizationRepository 
    : AbstractRepository<Organization, OrganizationId>, IOrganizationRepository
{
    public OrganizationRepository(
        SupportDeskDbContext context,
        IServiceProvider serviceProvider,
        IUserContext userContext,
        ITenantContext tenantContext)
        : base(context, serviceProvider, userContext, tenantContext)
    {
        
    }

    public async Task<Organization?> GetByNameAsync(OrganizationName name, CancellationToken cancellationToken = default)
    {
        return await Context.Organizations.FirstOrDefaultAsync(o => o.Name == name, cancellationToken);
    }
}