using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Repository;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Users;

public sealed class UserRepository 
    : AbstractRepository<User, UserId>, IUserRepository
{
    public UserRepository(
        SupportDeskDbContext context,
        IDomainEventCollector domainEventCollector) 
        : base(context, domainEventCollector)
    {
        
    }
}