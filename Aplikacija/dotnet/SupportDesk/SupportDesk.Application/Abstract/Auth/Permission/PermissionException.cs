namespace SupportDesk.Application.Abstract.Auth.Permission;

public class PermissionException : Exception
{
    public static string ErrorCode => "permission_denied";
    public string ErrorMessage { get; init; }

    public PermissionException(Permission permission)
    {
        ErrorMessage = permission.ErrorMessage;
    }
}