using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static class RolePermissions
{
    private static readonly Dictionary<UserRole, List<Permission>> DefaultPermissions = new()
    {
        [UserRole.Customer] =
        [
            Permissions.Tickets.Open,
            Permissions.Tickets.View,
            Permissions.Tickets.GiveFeedback,
            
            Permissions.Messages.Send,
            Permissions.Messages.Get,
            
            Permissions.Organizations.ViewAll
        ],
        [UserRole.OrganizationAdmin] =
        [
            Permissions.Categories.Add,
            Permissions.Categories.Update,
            Permissions.Categories.Delete,
            
            Permissions.Faqs.Add,
            Permissions.Faqs.Update,
            Permissions.Faqs.Delete,
            
            Permissions.TemplateAnswers.Add,
            Permissions.TemplateAnswers.Update,
            Permissions.TemplateAnswers.Delete,
            
            Permissions.Tickets.View,
            
            Permissions.Messages.Send,
            Permissions.Messages.Get,
            
            Permissions.Notes.Add,
            
            Permissions.SupportAgentInvites.Create,
            Permissions.SupportAgentInvites.Revoke,
            
            Permissions.Organizations.ViewAdminDashboard,
            Permissions.Organizations.ViewKnowledgeBase,
            Permissions.Organizations.ViewSupportAgentsSummary
        ],
        [UserRole.SupportAgent] =
        [
            Permissions.Tickets.View,
            Permissions.Tickets.Assign,
            Permissions.Tickets.Close,
            
            Permissions.Messages.Send,
            Permissions.Messages.Get,
            
            Permissions.Notes.Add
        ]
    };

    public static IReadOnlyCollection<Permission> GetDefaultPermissions(UserRole role)
    {
        return DefaultPermissions.TryGetValue(role, out var permissions) ? permissions : [];
    }
}