using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationTemplateAnswers.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationTemplateAnswers;

public sealed record GetOrganizationTemplateAnswersQueryResult(
    List<TemplateAnswerListingDto> TemplateAnswers
    ) : IQueryResult;