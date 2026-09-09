using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationTemplateAnswers.Dtos;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationTemplateAnswers;

internal sealed class GetOrganizationTemplateAnswersQueryHandler
    : AbstractQueryHandler<GetOrganizationTemplateAnswersQuery, GetOrganizationTemplateAnswersQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetOrganizationTemplateAnswersQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IApplicationDbContext applicationDbContext) 
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetOrganizationTemplateAnswersQueryResult> ExecuteAsync(GetOrganizationTemplateAnswersQuery query, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId((Guid)_tenantContext.GetCurrentOrganizationId()!);
        
        var templateAnswers = await _applicationDbContext.TemplateAnswers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(templateAnswer => templateAnswer.OrganizationId == organizationId)
            .OrderBy(templateAnswer => templateAnswer.Title)
            .Select(templateAnswer => new TemplateAnswerListingDto(
                templateAnswer.Id.IdValue,
                templateAnswer.Title.TitleValue,
                templateAnswer.Text.TextValue))
            .ToListAsync(cancellationToken);
        
        return new GetOrganizationTemplateAnswersQueryResult(templateAnswers);      
    }
}