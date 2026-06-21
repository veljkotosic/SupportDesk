using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.AddTemplateAnswer;

public record AddTemplateAnswerResult(Guid TemplateAnswerId) : IRequestResult;