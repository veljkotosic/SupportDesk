using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Faqs.Command.AddFaq;

public sealed record AddFaqCommandResult(Guid FaqId) : ICommandResult;