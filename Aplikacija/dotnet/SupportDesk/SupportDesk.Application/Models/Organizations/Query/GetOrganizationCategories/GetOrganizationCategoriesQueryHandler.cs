using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationCategories.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Organization.Validation;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationCategories;

internal sealed class GetOrganizationCategoriesQueryHandler
    : AbstractQueryHandler<GetOrganizationCategoriesQuery, GetOrganizationCategoriesQueryResult>
{
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetOrganizationCategoriesQueryHandler(
        PermissionChecker permissionChecker,
        IApplicationDbContext applicationDbContext) 
        : base(permissionChecker)
    {
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetOrganizationCategoriesQueryResult> ExecuteAsync(GetOrganizationCategoriesQuery query, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(query.OrganizationId);
        
        var organizationExists = await _applicationDbContext.Organizations
            .IgnoreQueryFilters()
            .AnyAsync(organization => organization.Id == organizationId, cancellationToken);

        if (!organizationExists)
        {
            throw new ValidationException(OrganizationErrors.NotFound(organizationId));
        }

        var categories = await _applicationDbContext.Categories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(category => category.OrganizationId == organizationId)
            .Select(category => new CategoryListingDto(
                category.Id.IdValue,
                category.Name.NameValue))
            .ToListAsync(cancellationToken);
        
        return new GetOrganizationCategoriesQueryResult(categories);
    }
}