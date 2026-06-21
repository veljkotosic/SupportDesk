using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Faq.UpdateFaq;

public record UpdateFaqRequest(Guid FaqId, string Question, string Answer) : IRequest;
