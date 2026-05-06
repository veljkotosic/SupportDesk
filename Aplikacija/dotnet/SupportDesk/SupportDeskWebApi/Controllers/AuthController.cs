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
    public async Task<ActionResult<SignupResult>> Signup(SignupRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }
    
    [AllowAnonymous]
    [HttpPost("refresh-login")]
    public async Task<ActionResult<RefreshLoginResult>> RefreshLogin(RefreshLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }
    
    [Authorize]
    [HttpDelete("logout")]
    public async Task<ActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpDelete("logout-all")]
    public async Task<ActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(new LogoutAllRequest(), cancellationToken);
        return Ok();
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<GetMeResult>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(new GetMeRequest(), cancellationToken);
        return result;
    }
}