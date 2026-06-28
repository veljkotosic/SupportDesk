using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetSettings;

public record GetSettingsResult(
    Guid OrganizationId,
    List<FaqSettingsDto> Faqs,
    List<TemplateAnswerSettingsDto> TemplateAnswers,
    List<CategorySettingsDto> Categories) 
    : IRequestResult;
