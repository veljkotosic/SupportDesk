using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;
using SupportDeskWebApi.Requests.Organization.ListCategories;

namespace SupportDeskWebApi.Data.Entities.Category.Repository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Context.Categories.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public Task<List<CategoryListingDto>> GetCategoriesByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return Context.Categories
            .IgnoreQueryFilters()
            .Where(c => c.OrganizationId == organizationId)
            .Select(c => new CategoryListingDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await Context.Categories
            .Where(c => c.Id == entity.Id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}