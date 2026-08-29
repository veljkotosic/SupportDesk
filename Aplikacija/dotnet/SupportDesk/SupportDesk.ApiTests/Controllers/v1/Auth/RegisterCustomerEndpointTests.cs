using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Models.Auth.Command.RegisterCustomer;

namespace SupportDesk.ApiTests.Controllers.v1.Auth;

[TestFixture]
internal sealed class RegisterCustomerEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/auth/registerCustomer";

    [Test]
    public async Task Post_WithValidData_ShouldReturn204NoContent_AndSetAuthCookies()
    {
        var command = new RegisterCustomerCommand(DefaultCustomerUserName, DefaultCustomerEmail, DefaultCustomerPassword);

        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
        AssertResponse.HasAuthCookiesSet(response);
    }
    
    [Test]
    public async Task Post_WithInvalidUserName_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var command = new RegisterCustomerCommand("", DefaultCustomerEmail, DefaultCustomerPassword);

        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Post_WithInvalidEmail_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var command = new RegisterCustomerCommand(DefaultCustomerUserName, "", DefaultCustomerPassword);

        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Post_WithInvalidPassword_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var command = new RegisterCustomerCommand(DefaultCustomerUserName, DefaultCustomerEmail, "");

        var response = await Client.PostAsJsonAsync(EndpointUrl, command);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
}