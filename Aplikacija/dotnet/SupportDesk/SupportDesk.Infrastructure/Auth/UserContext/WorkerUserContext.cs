using SupportDesk.Application.Abstract.Auth.UserContext;

namespace SupportDesk.Infrastructure.Auth.UserContext;

public sealed class WorkerUserContext : IUserContext, IUserContextSetter
{
    private Guid? _userId = null;
    
    public Guid GetCurrentUserId()
    {
        if (_userId is null)
        {
            throw new InvalidOperationException("No user context available for current worker operation.");
        }
        
        return _userId.Value;
    }

    public void SetCurrentUserId(Guid? userId)
    {
        _userId = userId;
    }
}