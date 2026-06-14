using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetSettings;

public record GetSettingsResult(
    List<FaqSettingsDto> Faqs,
    List<TemplateAnswerSettingsDto> TemplateAnswers,
    List<CategorySettingsDto> Categories) 
    : IRequestResult;
