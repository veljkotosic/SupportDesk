using SupportDeskWebApi.Data.Entities.Common.Repository;
using SupportDeskWebApi.Requests.Organization.ListCategories;

namespace SupportDeskWebApi.Data.Entities.Category.Repository;

public interface ICategoryRepository : IRepository<Category>, IDeleteRepository<Category>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<CategoryListingDto>> GetCategoriesByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
}