using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Permissions;

namespace SupportDesk.Application.Models.Categories.Command.DeleteCategory;

public record DeleteCategoryCommand(Guid CategoryId) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Categories.Delete
        ];
    }
}