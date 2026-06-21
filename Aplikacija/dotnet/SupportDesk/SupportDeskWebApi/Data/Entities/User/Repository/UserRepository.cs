using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.User.Repository;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(SupportDeskDbContext context) 
        : base(context)
    {
    }
}