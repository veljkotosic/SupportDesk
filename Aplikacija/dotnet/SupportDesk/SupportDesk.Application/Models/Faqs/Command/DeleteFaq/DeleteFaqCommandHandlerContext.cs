using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Faq.ValueObjects;

namespace SupportDesk.Application.Models.Faqs.Command.DeleteFaq;

internal sealed record DeleteFaqCommandHandlerContext(Faq? Faq, FaqId FaqId) : ICommandHandlerContext;