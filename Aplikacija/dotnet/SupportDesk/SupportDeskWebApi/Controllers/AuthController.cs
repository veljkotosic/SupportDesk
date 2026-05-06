using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.Auth.GetMe;
using SupportDeskWebApi.Requests.Auth.Login;
using SupportDeskWebApi.Requests.Auth.Logout;
using SupportDeskWebApi.Requests.Auth.LogoutAll;
using SupportDeskWebApi.Requests.Auth.RefreshLogin;
using SupportDeskWebApi.Requests.Auth.Signup;

namespace SupportDeskWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    
    public AuthController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<ActionResult> Signup(SignupRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.RefreshTokenExpirationDate);
        
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.RefreshTokenExpirationDate);
        
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("refresh-login")]
    public async Task<ActionResult> RefreshLogin(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized();
        }
        
        var result = await _dispatcher.ExecuteAsync(new RefreshLoginRequest(refreshToken), cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.RefreshTokenExpirationDate);
        
        return Ok();
    }
    
    [Authorize]
    [HttpDelete("logout")]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized();
        }
        
        await _dispatcher.ExecuteAsync(new LogoutRequest(refreshToken), cancellationToken);
        ClearTokenCookies();
        
        return Ok();
    }

    [Authorize]
    [HttpDelete("logout-all")]
    public async Task<ActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(new LogoutAllRequest(), cancellationToken);
        ClearTokenCookies();
        
        return Ok();
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<GetMeResult>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(new GetMeRequest(), cancellationToken);
        return result;
    }
    
    private void SetTokenCookies(string accessToken, string refreshToken, DateTime refreshTokenExpiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, 
            SameSite = SameSiteMode.Strict,
            Expires = refreshTokenExpiresAt
        };

        Response.Cookies.Append("accessToken", accessToken, cookieOptions);
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private void ClearTokenCookies()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
    }
}