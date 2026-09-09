namespace SupportDesk.Infrastructure.Persistence.RefreshToken;

public sealed class RefreshTokenException : Exception
{
    public const string InvalidTokenCode = "invalid_token";
    private const string InvalidTokenMessage = "Invalid token";
    
    public const string TokenExpiredCode = "token_expired";
    private const string TokenExpiredMessage = "Token expired";
    
    public string ErrorCode { get; init;}
    public string ErrorMessage { get; init; }
    
    public RefreshTokenException(string errorCode, string errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    
    public static RefreshTokenException InvalidToken() => new(InvalidTokenCode, InvalidTokenMessage);
    public static RefreshTokenException TokenExpired() => new(TokenExpiredCode, TokenExpiredMessage);
}