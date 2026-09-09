using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.User.Validation;

public sealed class UserErrors : AbstractErrors<User, UserId>
{
    public static ValidationError InvalidCredentials()
    {
        string code = "invalid_credentials";
        string message = "Invalid credentials";

        return CreateValidationError(code, message);
    }
    
    public static ValidationError RoleDoesNotExist(string roleName)
    {
        string code = "user_role_does_not_exist";
        string message = $"Role '{roleName}' does not exist";
    
        return CreateValidationError(code, message);
    }
}