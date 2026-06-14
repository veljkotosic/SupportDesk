using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.UpdateTemplateAnswer;

public record UpdateTemplateAnswerRequest(Guid TemplateAnswerId, string Title, string Text) : IRequest;
