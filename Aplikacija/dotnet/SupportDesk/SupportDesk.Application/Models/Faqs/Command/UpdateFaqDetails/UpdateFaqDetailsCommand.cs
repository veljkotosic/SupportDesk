using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Faqs.Command.UpdateFaqDetails;

public sealed record UpdateFaqDetailsCommand(
    Guid FaqId,
    string? Question,
    string? Answer
    ) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return
        [
            Permissions.Faqs.Update
        ];
    }
}