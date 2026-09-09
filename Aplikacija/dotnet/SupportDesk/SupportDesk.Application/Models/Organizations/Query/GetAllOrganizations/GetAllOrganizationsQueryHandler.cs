using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetAllOrganizations.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetAllOrganizations;

internal sealed class GetAllOrganizationsQueryHandler
    : AbstractQueryHandler<GetAllOrganizationsQuery, GetAllOrganizationsQueryResult>
{
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetAllOrganizationsQueryHandler(
        PermissionChecker permissionChecker,
        IApplicationDbContext applicationDbContext) 
        : base(permissionChecker)
    {
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetAllOrganizationsQueryResult> ExecuteAsync(GetAllOrganizationsQuery query, CancellationToken cancellationToken)
    {
        var organizationListings = await _applicationDbContext.Organizations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(organization => new OrganizationListingDto(
                organization.Id.IdValue,
                organization.Name.NameValue))
            .ToListAsync(cancellationToken);
        
        return new GetAllOrganizationsQueryResult(organizationListings);
    }
}