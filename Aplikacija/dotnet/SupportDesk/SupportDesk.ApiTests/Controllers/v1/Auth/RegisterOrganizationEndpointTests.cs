using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Models.Auth.Command.RegisterOrganization;

namespace SupportDesk.ApiTests.Controllers.v1.Auth;

[TestFixture]
internal sealed class RegisterOrganizationEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/auth/registerOrganization";
    
    [Test]
    public async Task Post_WithValidData_ShouldReturn204NoContent_AndSetAuthCookies()
    {
        var command = new RegisterOrganizationCommand(
            DefaultOrganizationAdminUserName,
            DefaultOrganizationName,
            DefaultOrganizationAdminEmail,
            DefaultOrganizationAdminPassword);

        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
        AssertResponse.HasAuthCookiesSet(response);
    }
    
    [Test]
    public async Task Post_WithInvalidUserName_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var command = new RegisterOrganizationCommand(
            "", 
            DefaultOrganizationName,
            DefaultOrganizationAdminEmail,
            DefaultOrganizationAdminPassword);
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Post_WithInvalidOrganizationName_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var command = new RegisterOrganizationCommand(
            DefaultOrganizationAdminUserName, 
            "",
            DefaultOrganizationAdminEmail,
            DefaultOrganizationAdminPassword);
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Post_WithInvalidEmail_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var command = new RegisterOrganizationCommand(
            DefaultOrganizationAdminUserName, 
            DefaultOrganizationName,
            "",
            DefaultOrganizationAdminPassword);
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Post_WithInvalidPassword_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var command = new RegisterOrganizationCommand(
            DefaultOrganizationAdminUserName, 
            DefaultOrganizationName,
            DefaultOrganizationAdminEmail,
            "");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
}