using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.Category.Repository;

public interface ICategoryRepository : IRepository<Category>, IDeleteRepository<Category>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}