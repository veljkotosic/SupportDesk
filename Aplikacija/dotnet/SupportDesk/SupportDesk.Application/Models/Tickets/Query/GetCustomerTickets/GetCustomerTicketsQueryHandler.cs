using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Abstract.ReadModels;

namespace SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;

internal sealed class GetCustomerTicketsQueryHandler
    : AbstractQueryHandler<GetCustomerTicketsQuery, GetCustomerTicketsQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly ITicketReadModelService _ticketReadModelService;

    public GetCustomerTicketsQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        ITicketReadModelService ticketReadModelService)
        : base(permissionChecker)
    {
        _userContext = userContext;
        _ticketReadModelService = ticketReadModelService;
    }

    protected override async Task<GetCustomerTicketsQueryResult> ExecuteAsync(
        GetCustomerTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var customerId = _userContext.GetCurrentUserId();

        return await _ticketReadModelService.GetCustomerTicketsAsync(
            query,
            customerId,
            cancellationToken);
    }
}