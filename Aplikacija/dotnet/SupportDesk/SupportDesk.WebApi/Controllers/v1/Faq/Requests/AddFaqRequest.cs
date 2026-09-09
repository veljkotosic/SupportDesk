namespace SupportDesk.WebApi.Controllers.v1.Faq.Requests;

public sealed record AddFaqRequest(string Question, string Answer);