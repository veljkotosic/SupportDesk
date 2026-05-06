namespace SupportDeskWebApi.Data.Database.RefreshToken;

public class RefreshTokenException : Exception
{
    private const string InvalidTokenMessage = "Invalid token";
    private const string TokenExpiredMessage = "Token expired";
    
    public RefreshTokenException(string message)
        : base(message)
    {
        
    }
    
    public static RefreshTokenException InvalidToken() => new(InvalidTokenMessage);
    public static RefreshTokenException TokenExpired() => new(TokenExpiredMessage);
}