using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.DeleteTemplateAnswer;

internal sealed record DeleteTemplateAnswerCommandHandlerContext(
    TemplateAnswer? TemplateAnswer,
    TemplateAnswerId TemplateAnswerId
    ) : ICommandHandlerContext;