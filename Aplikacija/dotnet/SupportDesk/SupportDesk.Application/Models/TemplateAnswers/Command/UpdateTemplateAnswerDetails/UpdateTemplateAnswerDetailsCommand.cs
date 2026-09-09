using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.UpdateTemplateAnswerDetails;

public sealed record UpdateTemplateAnswerDetailsCommand(
    Guid TemplateAnswerId,
    string? Title,
    string? Text
    ) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.TemplateAnswers.Update
        ];
    }
}