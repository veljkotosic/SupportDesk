using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.UpdateTemplateAnswerDetails;

internal sealed record UpdateTemplateAnswerDetailsCommandHandlerContext(
    TemplateAnswer? TemplateAnswer,
    TemplateAnswerId TemplateAnswerId
    ) : ICommandHandlerContext;