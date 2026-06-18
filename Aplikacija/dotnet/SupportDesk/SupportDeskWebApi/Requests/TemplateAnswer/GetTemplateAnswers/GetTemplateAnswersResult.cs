using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.GetTemplateAnswers;

public record GetTemplateAnswersResult(List<TemplateAnswerDto> TemplateAnswers) : IRequestResult;
