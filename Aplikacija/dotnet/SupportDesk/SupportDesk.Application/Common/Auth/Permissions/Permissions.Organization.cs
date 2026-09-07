using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static partial class Permissions
{
    public static class Organizations
    {
        public static readonly Permission ViewAdminDashboard = new("Organizations.ViewAdminDashboard", "You don't have permission to view the admin dashboard.");      
        public static readonly Permission ViewKnowledgeBase = new("Organizations.ViewKnowledgeBase", "You don't have permission to view the knowledge base.");     
        public static readonly Permission ViewSupportAgentsSummary = new("Organizations.ViewSupportAgentsSummary", "You don't have permission to view the support agents summary.");    
    }
}