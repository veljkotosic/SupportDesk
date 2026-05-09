using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Faq.RemoveFaq;

public record RemoveFaqRequest(Guid FaqId) : IRequest;