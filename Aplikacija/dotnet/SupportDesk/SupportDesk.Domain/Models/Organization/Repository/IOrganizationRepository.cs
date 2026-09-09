using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Domain.Models.Organization.Repository;

public interface IOrganizationRepository : IAbstractRepository<Organization, OrganizationId>
{
    Task<Organization?> GetByNameAsync(OrganizationName name, CancellationToken cancellationToken = default);
}