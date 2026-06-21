namespace SupportDeskWebApi.Auth.AuthService;

public class AuthException : Exception
{
    private const string InvalidCredentialsMessage = "Invalid credentials";
    private const string CannotLogoutOtherUserMessage = "Cannot logout other user";
    private const string UserNotFoundMessage = "User not found";
    private const string OrganizationAlreadyExistMessage = "Organization already exist";
    private const string InvalidInviteCodeMessage = "Invalid invite code";
    private const string RegistrationFailedMessage = "Registration failed";
    
    public AuthException(string message)
        : base(message)
    {
        
    }

    public AuthException(string message, IReadOnlyList<string> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; } = [];
    
    public static AuthException InvalidCredentials() => new(InvalidCredentialsMessage);
    public static AuthException CannotLogoutOtherUser() => new(CannotLogoutOtherUserMessage);
    public static AuthException UserNotFound() => new(UserNotFoundMessage);
    public static AuthException OrganizationAlreadyExist() => new(OrganizationAlreadyExistMessage);
    public static AuthException InvalidInviteCode() => new(InvalidInviteCodeMessage);
    public static AuthException RegistrationFailed(IReadOnlyList<string> errors) => new(
        errors.FirstOrDefault() ?? RegistrationFailedMessage,
        errors);
}
