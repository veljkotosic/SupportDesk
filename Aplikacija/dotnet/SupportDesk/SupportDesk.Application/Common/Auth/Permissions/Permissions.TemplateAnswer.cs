using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static partial class Permissions
{
    public static class TemplateAnswers
    {
        public static readonly Permission Add = new("TemplateAnswers.Add", "You don't have permission to add a template answer.");       
        public static readonly Permission Update = new("TemplateAnswers.Update", "You don't have permission to update a template answer.");       
        public static readonly Permission Delete = new("TemplateAnswers.Delete", "You don't have permission to delete a template answer.");      
    }
}