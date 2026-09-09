using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Faqs.Command.DeleteFaq;

public sealed record DeleteFaqCommand(Guid FaqId) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Faqs.Delete
        ];
    }
}