using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Message.Validation;
using SupportDesk.Domain.Models.Message.ValueObjects;

namespace SupportDesk.Application.Models.Messages.Query.GetMessageDetails;

internal sealed class GetMessageDetailsQueryHandler
    : AbstractQueryHandler<GetMessageDetailsQuery, GetMessageDetailsQueryResult>
{
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetMessageDetailsQueryHandler(
        PermissionChecker permissionChecker,
        IApplicationDbContext applicationDbContext) 
        : base(permissionChecker)
    {
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetMessageDetailsQueryResult> ExecuteAsync(GetMessageDetailsQuery query, CancellationToken cancellationToken)
    {
        var messageId = new MessageId(query.MessageId);

        var queryable =
            from message in _applicationDbContext.Messages.AsNoTracking()
            where message.Id == messageId
            
            join sender in _applicationDbContext.DomainUsers.AsNoTracking()
                on message.SenderId equals sender.Id

            select new GetMessageDetailsQueryResult(
                message.Id.IdValue,
                sender.Id.IdValue,
                sender.UserName.UserNameValue,
                message.Text.TextValue,
                message.CreatedAt.CreatedAtValue);

        var result = await queryable.FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new ValidationException(MessageErrors.NotFound(messageId));
        }
        
        return result;
    }
}