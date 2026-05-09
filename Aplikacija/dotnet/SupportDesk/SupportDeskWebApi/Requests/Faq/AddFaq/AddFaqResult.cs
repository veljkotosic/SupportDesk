using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Faq.AddFaq;

public record AddFaqResult(Guid FaqId) : IRequestResult;