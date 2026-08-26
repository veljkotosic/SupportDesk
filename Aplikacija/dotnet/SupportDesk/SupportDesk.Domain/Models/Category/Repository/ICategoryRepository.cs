using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Domain.Models.Category.Repository;

public interface ICategoryRepository 
    : IAbstractRepository<Category, CategoryId>
{
    Task<Category?> GetByNameAsync(CategoryName name, CancellationToken cancellationToken = default);
}