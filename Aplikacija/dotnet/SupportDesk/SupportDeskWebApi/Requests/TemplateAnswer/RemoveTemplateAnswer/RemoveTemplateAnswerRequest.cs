using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.RemoveTemplateAnswer;

public record RemoveTemplateAnswerRequest(Guid TemplateAnswerId) : IRequest;