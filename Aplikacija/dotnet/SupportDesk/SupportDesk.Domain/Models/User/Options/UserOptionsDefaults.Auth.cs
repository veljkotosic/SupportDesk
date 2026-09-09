namespace SupportDesk.Domain.Models.User.Options;

public static partial class UserOptionsDefaults
{
    public const int PasswordRequiredLength = 8;
    public const bool PasswordRequireDigit = true;
    public const bool PasswordRequireLowercase = true;
    public const bool PasswordRequireUppercase = true;
    public const bool PasswordRequireNonAlphanumeric = true;
    
    public const bool UserRequireUniqueEmail = true;
}