using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.User.Repository;

public interface IUserRepository : IAbstractRepository<User, UserId>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
}