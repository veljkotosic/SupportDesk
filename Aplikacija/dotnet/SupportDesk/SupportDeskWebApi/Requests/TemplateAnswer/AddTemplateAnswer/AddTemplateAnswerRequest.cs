using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.AddTemplateAnswer;

public record AddTemplateAnswerRequest(
    string Title,
    string Text)
    : IRequest<AddTemplateAnswerResult>;