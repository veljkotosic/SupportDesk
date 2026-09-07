using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase;

public record GetOrganizationKnowledgeBaseQueryResult(
    Guid OrganizationId,
    List<KnowledgeBaseFaqDto> Faqs,
    List<KnowledgeBaseTemplateAnswerDto> TemplateAnswers,
    List<KnowledgeBaseCategoryDto> Categories
    ) : IQueryResult;