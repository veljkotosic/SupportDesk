using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.Repository;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Categories;

public sealed class CategoryRepository
    : AbstractRepository<Category, CategoryId>, ICategoryRepository
{
    public CategoryRepository(
        SupportDeskDbContext context,
        IServiceProvider serviceProvider,
        IUserContext userContext,
        ITenantContext tenantContext)
        : base(context, serviceProvider, userContext, tenantContext)
    {
        
    }

    public async Task<Category?> GetByNameAsync(CategoryName name, CancellationToken cancellationToken = default)
    {
        return await Context.Categories.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<Category?> GetByIdAndOrganizationIdAsync(CategoryId id, OrganizationId organizationId, CancellationToken cancellationToken = default)
    {
        return await Context.Categories
            .IgnoreQueryFilters([QueryFilterKeys.TenantIsolationFilter])
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == organizationId, cancellationToken);
    }
}