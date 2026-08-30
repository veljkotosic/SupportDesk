using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Faqs.Command.AddFaq;

public sealed record AddFaqCommand(string Question, string Answer) : ICommand<AddFaqCommandResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Faqs.Add
        ];
    }
}