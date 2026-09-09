using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Common.Auth;
using SupportDesk.Application.Models.Auth.Command.Login;
using SupportDesk.Application.Models.Auth.Command.Logout;
using SupportDesk.Application.Models.Auth.Command.LogoutAll;
using SupportDesk.Application.Models.Auth.Command.RefreshLogin;
using SupportDesk.Application.Models.Auth.Command.RegisterCustomer;
using SupportDesk.Application.Models.Auth.Command.RegisterOrganization;
using SupportDesk.Application.Models.Auth.Command.RegisterSupportAgent;
using SupportDesk.Application.Models.Auth.Query.GetMe;
using SupportDesk.WebApi.Controllers.v1.Auth.Requests;

namespace SupportDesk.WebApi.Controllers.v1.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public AuthController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(GetMeQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GetMeQueryResult>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.DispatchAsync(new GetMeQuery(), cancellationToken);
        
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        
        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        
        return NoContent();
    }
    
    [AllowAnonymous]
    [HttpPost("refreshLogin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RefreshLogin(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized();
        }
        
        var result = await _commandDispatcher.DispatchAsync(new RefreshLoginCommand(refreshToken), cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        
        return NoContent();
    }

    [Authorize]
    [HttpDelete("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized();
        }
             
        await _commandDispatcher.DispatchAsync(new LogoutCommand(refreshToken), cancellationToken);
        
        ClearTokenCookies();
        
        return NoContent();
    }

    [Authorize]
    [HttpDelete("logoutAll")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        await _commandDispatcher.DispatchAsync(new LogoutAllCommand(), cancellationToken);
        ClearTokenCookies();
        
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("registerCustomer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterCustomer(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(
            request.UserName,
            request.Email,
            request.Password);
        
        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        
        return NoContent();
    }
    
    [AllowAnonymous]
    [HttpPost("registerSupportAgent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterSupportAgent(
        [FromBody] RegisterSupportAgentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterSupportAgentCommand(
            request.UserName,
            request.Email,
            request.Password,
            request.Code);
        
        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("registerOrganization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterOrganization(
        [FromBody] RegisterOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterOrganizationCommand(
            request.UserName,
            request.OrganizationName,
            request.Email,
            request.Password);
        
        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        
        return NoContent();
    }
    
    private void SetTokenCookies(AccessToken accessToken, RefreshToken refreshToken)
    {
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, 
            SameSite = SameSiteMode.Strict,
            Expires = accessToken.ExpiresAt
        };
        
        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, 
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.ExpiresAt
        };

        Response.Cookies.Append("accessToken", accessToken.Value, accessTokenOptions);
        Response.Cookies.Append("refreshToken", refreshToken.Value, refreshTokenOptions);
    }

    private void ClearTokenCookies()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
    }
}