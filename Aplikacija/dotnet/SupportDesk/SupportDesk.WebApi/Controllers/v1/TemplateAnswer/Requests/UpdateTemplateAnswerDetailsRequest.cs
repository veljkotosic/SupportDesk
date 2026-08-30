using SupportDesk.WebApi.Abstract.Request;

namespace SupportDesk.WebApi.Controllers.v1.TemplateAnswer.Requests;

public sealed record UpdateTemplateAnswerDetailsRequest(string? Title, string? Text) : IAtLeastOneFieldRequiredRequest;