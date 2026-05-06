namespace SupportDeskWebApi.Auth.Jwt;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string Key { get; init; } = null!;
    public int ExpirationMinutes { get; init; } 
    public int CustomerRefreshTokenExpirationDays { get; init; }
    public int OrganizationRefreshTokenExpirationDays { get; init; }
}