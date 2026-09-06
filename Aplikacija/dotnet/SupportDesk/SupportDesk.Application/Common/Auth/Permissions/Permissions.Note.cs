using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static partial class Permissions
{
    public static class Notes
    {
        public static readonly Permission Add = new("Notes.Add", "You don't have permission to add a note.");       
    }
}