using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.User.Repository;

public interface IUserRepository : IAbstractRepository<User, UserId>
{
    
}