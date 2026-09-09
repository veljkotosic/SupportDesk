using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Common.Auth;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.Infrastructure.Auth.Jwt;

public class JwtTokenProvider : ITokenProvider
{
    private readonly JwtSettings _jwtSettings;
    
    public JwtTokenProvider(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }
    
    public AccessToken GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.IdValue.ToString()),
            new Claim(ClaimTypes.Email, user.Email.EmailValue),
            new Claim(ClaimTypes.Name, user.UserName.UserNameValue),
        };

        if (user.OrganizationId is not null)
        {
            claims.Add(new Claim("organizationId", user.OrganizationId.IdValue.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);
        
        var accessToken = new AccessToken(
            new JwtSecurityTokenHandler().WriteToken(token), 
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes));
        
        return accessToken;
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}