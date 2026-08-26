using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User.Validation;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Auth.Query.GetMe;

public sealed class GetMeQueryHandler : AbstractQueryHandler<GetMeQuery, GetMeQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetMeQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext, 
        IApplicationDbContext applicationDbContext) 
        : base(permissionChecker)
    {
        _userContext = userContext;
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetMeQueryResult> ExecuteAsync(GetMeQuery query, CancellationToken cancellationToken)
    {
        var userId = new UserId(_userContext.GetCurrentUserId());

        var result = await _applicationDbContext.DomainUsers
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new GetMeQueryResult(
                user.Id.IdValue,
                user.UserName.UserNameValue,
                user.Email.EmailValue,
                user.Role))
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new ValidationException(UserErrors.NotFound(userId));
        }

        return result;
    }
}