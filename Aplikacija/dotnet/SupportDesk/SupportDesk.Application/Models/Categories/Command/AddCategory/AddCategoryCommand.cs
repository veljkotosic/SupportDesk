using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Categories.Command.AddCategory;

public sealed record AddCategoryCommand(string Name, string Description) : ICommand<AddCategoryCommandResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Categories.Add
        ];
    }
}