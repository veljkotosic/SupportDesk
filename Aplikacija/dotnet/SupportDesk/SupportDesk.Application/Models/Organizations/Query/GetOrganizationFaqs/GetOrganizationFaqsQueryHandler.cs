using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Organization.Validation;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs;

internal sealed class GetOrganizationFaqsQueryHandler
    : AbstractQueryHandler<GetOrganizationFaqsQuery, GetOrganizationFaqsQueryResult>
{
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetOrganizationFaqsQueryHandler(
        PermissionChecker permissionChecker,
        IApplicationDbContext applicationDbContext)
        : base(permissionChecker)
    {
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetOrganizationFaqsQueryResult> ExecuteAsync(GetOrganizationFaqsQuery query, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(query.OrganizationId);
        
        var organizationExists = await _applicationDbContext.Organizations
            .IgnoreQueryFilters()
            .AnyAsync(organization => organization.Id == organizationId, cancellationToken);

        if (!organizationExists)
        {
            throw new ValidationException(OrganizationErrors.NotFound(organizationId));
        }

        var faqs = await _applicationDbContext.Faqs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(faq => faq.OrganizationId == organizationId)
            .Select(faq => new FaqListingDto(
                faq.Id.IdValue,
                faq.Question.QuestionValue,
                faq.Answer.AnswerValue))
            .ToListAsync(cancellationToken);
        
        return new GetOrganizationFaqsQueryResult(faqs);       
    }
}