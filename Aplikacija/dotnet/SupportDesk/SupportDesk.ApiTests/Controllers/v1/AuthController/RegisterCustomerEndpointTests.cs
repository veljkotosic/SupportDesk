using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Models.Auth.Command.RegisterCustomer;
using SupportDesk.WebApi.Controllers.v1.Auth.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.AuthController;

[TestFixture]
internal sealed class RegisterCustomerEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/auth/registerCustomer";

    [Test]
    public async Task Post_WithValidData_ShouldReturn204NoContent_AndSetAuthCookies()
    {
        var request = new RegisterCustomerRequest(DefaultCustomerUserName, DefaultCustomerEmail, DefaultCustomerPassword);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
        AssertResponse.HasAuthCookiesSet(response);
    }
    
    [Test]
    public async Task Post_WithInvalidUserName_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var request = new RegisterCustomerRequest("", DefaultCustomerEmail, DefaultCustomerPassword);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Post_WithInvalidEmail_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var request = new RegisterCustomerRequest(DefaultCustomerUserName, "", DefaultCustomerPassword);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Post_WithInvalidPassword_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var request = new RegisterCustomerRequest(DefaultCustomerUserName, DefaultCustomerEmail, "");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
}