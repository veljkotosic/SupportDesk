using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User.Validation;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Auth.Query.GetMe;

internal sealed class GetMeQueryHandler : AbstractQueryHandler<GetMeQuery, GetMeQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly IApplicationDbContext _dbContext;
    
    public GetMeQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext, 
        IApplicationDbContext dbContext) 
        : base(permissionChecker)
    {
        _userContext = userContext;
        _dbContext = dbContext;
    }

    protected override async Task<GetMeQueryResult> ExecuteAsync(GetMeQuery query, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.GetCurrentUserId();
        var userId = new UserId(currentUserId);

        var user = await _dbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new ValidationException(UserErrors.NotFound(userId));
        }

        return new GetMeQueryResult(user.Id.IdValue, user.UserName, user.Email.EmailValue, user.Role);
    }
}