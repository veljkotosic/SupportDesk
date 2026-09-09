using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Domain.Models.Category.Repository;

public interface ICategoryRepository 
    : IAbstractRepository<Category, CategoryId>
{
    Task<Category?> GetByNameAsync(CategoryName name, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAndOrganizationIdAsync(CategoryId id, OrganizationId organizationId, CancellationToken cancellationToken = default);
}