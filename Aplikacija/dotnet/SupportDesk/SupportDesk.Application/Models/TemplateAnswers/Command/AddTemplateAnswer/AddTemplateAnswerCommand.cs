using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.AddTemplateAnswer;

public sealed record AddTemplateAnswerCommand(string Title, string Text)
    : ICommand<AddTemplateAnswerCommandResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.TemplateAnswers.Add
        ];
    }
}