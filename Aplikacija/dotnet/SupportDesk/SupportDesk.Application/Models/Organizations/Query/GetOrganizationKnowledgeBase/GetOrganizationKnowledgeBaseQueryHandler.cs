using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase;

internal sealed class GetOrganizationKnowledgeBaseQueryHandler
    : AbstractQueryHandler<GetOrganizationKnowledgeBaseQuery, GetOrganizationKnowledgeBaseQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetOrganizationKnowledgeBaseQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IApplicationDbContext applicationDbContext) 
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetOrganizationKnowledgeBaseQueryResult> ExecuteAsync(GetOrganizationKnowledgeBaseQuery query, CancellationToken cancellationToken)
    {
        var faqs = await _applicationDbContext.Faqs
            .AsNoTracking()
            .OrderBy(faq => faq.Question)
            .Select(faq => new KnowledgeBaseFaqDto(
                faq.Id.IdValue,
                faq.Question.QuestionValue,
                faq.Answer.AnswerValue))
            .ToListAsync(cancellationToken);
        
        var templateAnswers = await _applicationDbContext.TemplateAnswers
            .AsNoTracking()
            .OrderBy(template => template.Title)
            .Select(template => new KnowledgeBaseTemplateAnswerDto(
                template.Id.IdValue,
                template.Title.TitleValue,
                template.Text.TextValue))
            .ToListAsync(cancellationToken);
        
        var categories = await _applicationDbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new KnowledgeBaseCategoryDto(
                category.Id.IdValue,
                category.Name.NameValue,
                category.Description.DescriptionValue,
                _applicationDbContext.Tickets.Count(ticket => ticket.CategoryId == category.Id)))
            .ToListAsync(cancellationToken);

        return new GetOrganizationKnowledgeBaseQueryResult(
            (Guid)_tenantContext.GetCurrentOrganizationId()!,
            faqs,
            templateAnswers,
            categories);
    }
}