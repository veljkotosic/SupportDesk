using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

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

    public async Task DeleteAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await Context.Categories
            .Where(c => c.Id == entity.Id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}