using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.DeleteTemplateAnswer;

public sealed record DeleteTemplateAnswerCommand(Guid TemplateAnswerId) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.TemplateAnswers.Delete
        ];
    }
}