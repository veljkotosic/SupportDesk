using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.AddTemplateAnswer;

public sealed record AddTemplateAnswerCommandResult(Guid TemplateAnswerId) : ICommandResult;