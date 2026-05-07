namespace SupportDeskWebApi.Auth.AuthService;

public class AuthException : Exception
{
    private const string InvalidCredentialsMessage = "Invalid credentials";
    private const string CannotLogoutOtherUserMessage = "Cannot logout other user";
    private const string UserNotFoundMessage = "User not found";
    private const string OrganizationAlreadyExistMessage = "Organization already exist";
    private const string InvalidInviteCodeMessage = "Invalid invite code";
    
    public AuthException(string message)
        : base(message)
    {
        
    }
    
    public static AuthException InvalidCredentials() => new(InvalidCredentialsMessage);
    public static AuthException CannotLogoutOtherUser() => new(CannotLogoutOtherUserMessage);
    public static AuthException UserNotFound() => new(UserNotFoundMessage);
    public static AuthException OrganizationAlreadyExist() => new(OrganizationAlreadyExistMessage);
    public static AuthException InvalidInviteCode() => new(InvalidInviteCodeMessage);
}