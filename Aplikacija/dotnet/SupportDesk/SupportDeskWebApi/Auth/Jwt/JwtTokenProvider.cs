using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Entities.User;

namespace SupportDeskWebApi.Auth.Jwt;

public class JwtTokenProvider : ITokenProvider
{
    private readonly JwtSettings _jwtSettings;
    
    public JwtTokenProvider(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }
    
    public string GenerateAccessToken(User user, IList<UserRole> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
        };

        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("organizationId", user.OrganizationId.Value.ToString()));
        }
        
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}