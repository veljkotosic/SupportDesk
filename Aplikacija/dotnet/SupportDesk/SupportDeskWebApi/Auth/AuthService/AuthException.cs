namespace SupportDeskWebApi.Auth.AuthService;

public class AuthException : Exception
{
    private const string InvalidCredentialsMessage = "Invalid credentials";
    private const string CannotLogoutOtherUserMessage = "Cannot logout other user";
    private const string UserNotFoundMessage = "User not found";
    
    public AuthException(string message)
        : base(message)
    {
        
    }
    
    public static AuthException InvalidCredentials() => new(InvalidCredentialsMessage);
    public static AuthException CannotLogoutOtherUser() => new(CannotLogoutOtherUserMessage);
    public static AuthException UserNotFound() => new(UserNotFoundMessage);
}