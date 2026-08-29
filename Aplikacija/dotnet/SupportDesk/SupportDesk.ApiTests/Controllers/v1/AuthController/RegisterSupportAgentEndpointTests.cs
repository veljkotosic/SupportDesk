using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Models.Auth.Command.RegisterSupportAgent;
using SupportDesk.Domain.Models.User;
using SupportDesk.WebApi.Controllers.v1.Auth.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.AuthController;

[TestFixture]
internal sealed class RegisterSupportAgentEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/auth/registerSupportAgent";
    private User _orgAdmin = null!;

    [SetUp]
    public async Task TestSetup()
    {
        _orgAdmin = await SeedOrganizationAdmin();
    }

    [Test]
    public async Task Post_WithValidData_ShouldReturn204NoContent_AndSetAuthCookies()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _orgAdmin.OrganizationId!.IdValue);

        var request = new RegisterSupportAgentRequest(
            DefaultSupportAgentUserName,
            DefaultSupportAgentEmail,
            DefaultSupportAgentPassword,
            invite.Code.CodeValue);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
        AssertResponse.HasAuthCookiesSet(response);
    }

    [Test]
    public async Task Post_WithNonExistentInviteCode_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var request = new RegisterSupportAgentRequest(
            DefaultSupportAgentUserName,
            DefaultSupportAgentEmail,
            DefaultSupportAgentPassword,
            Guid.NewGuid());

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Post_WithMismatchedEmail_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _orgAdmin.OrganizationId!.IdValue);

        var request = new RegisterSupportAgentRequest(
            DefaultSupportAgentUserName,
            "different@email.com",
            DefaultSupportAgentPassword,
            invite.Code.CodeValue);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Post_WithInvalidUserName_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _orgAdmin.OrganizationId!.IdValue);

        var request = new RegisterSupportAgentRequest(
            "",
            DefaultSupportAgentEmail,
            DefaultSupportAgentPassword,
            invite.Code.CodeValue);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Post_WithInvalidEmail_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _orgAdmin.OrganizationId!.IdValue);

        var request = new RegisterSupportAgentRequest(
            DefaultSupportAgentUserName,
            "",
            DefaultSupportAgentPassword,
            invite.Code.CodeValue);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Post_WithInvalidPassword_ShouldReturn400BadRequest_WithProblemDetails()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _orgAdmin.OrganizationId!.IdValue);

        var request = new RegisterSupportAgentRequest(
            DefaultSupportAgentUserName,
            DefaultSupportAgentEmail,
            "",
            invite.Code.CodeValue);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);

        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
}