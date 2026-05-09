using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Faq.AddFaq;

public record AddFaqRequest(string Question, string Answer) : IRequest<AddFaqResult>;