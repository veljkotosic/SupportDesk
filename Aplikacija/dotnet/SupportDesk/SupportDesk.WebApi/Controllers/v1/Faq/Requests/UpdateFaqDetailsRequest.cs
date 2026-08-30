using SupportDesk.WebApi.Abstract.Request;

namespace SupportDesk.WebApi.Controllers.v1.Faq.Requests;

public sealed record UpdateFaqDetailsRequest(string? Question, string? Answer) : IAtLeastOneFieldRequiredRequest;