using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Faq.ValueObjects;

namespace SupportDesk.Application.Models.Faqs.Command.UpdateFaqDetails;

internal sealed record UpdateFaqDetailsCommandHandlerContext(
    Faq? Faq,
    FaqId FaqId
    ) : ICommandHandlerContext;