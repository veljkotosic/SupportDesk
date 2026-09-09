using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Categories.Command.UpdateCategoryDetails;

public sealed record UpdateCategoryDetailsCommand(
    Guid CategoryId,
    string? Name,
    string? Description) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Categories.Update
        ];
    }
}