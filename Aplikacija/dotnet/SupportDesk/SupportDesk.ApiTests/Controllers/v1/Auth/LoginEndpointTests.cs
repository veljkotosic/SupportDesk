using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Models.Auth.Command.Login;

namespace SupportDesk.ApiTests.Controllers.v1.Auth;

[TestFixture]
internal sealed class LoginEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/auth/login";

    [Test]
    public async Task Post_WithValidData_ShouldReturn204NoContent_AndSetAuthCookies()
    {
        _ = await SeedCustomerAsync();
        
        var loginCommand = new LoginCommand(DefaultCustomerEmail, DefaultCustomerPassword);

        var response = await Client.PostAsJsonAsync(EndpointUrl, loginCommand);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
        AssertResponse.HasAuthCookiesSet(response);
    }
    
    [Test]
    public async Task Post_WithInvalidEmail_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var loginCommand = new LoginCommand("", DefaultCustomerPassword);
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, loginCommand);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Post_WithInvalidPassword_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var loginCommand = new LoginCommand(DefaultCustomerEmail, "");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, loginCommand);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
}