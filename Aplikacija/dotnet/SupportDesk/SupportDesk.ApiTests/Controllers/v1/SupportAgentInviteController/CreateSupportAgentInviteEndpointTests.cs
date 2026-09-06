using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.SupportAgentInvites.Command.CreateSupportAgentInvite;
using SupportDesk.Domain.Models.User;
using SupportDesk.WebApi.Controllers.v1.SupportAgentInvite.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.SupportAgentInviteController;

[TestFixture]
internal sealed class CreateSupportAgentInviteEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/supportAgentInvite";

    private User _organizationAdmin;
    
    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await SeedOrganizationAdmin();
    }

    [Test]
    public async Task Post_WithValidData_ShouldReturn201Created()
    {
        await AuthenticateAs(_organizationAdmin);
        
        var request = new CreateSupportAgentInviteRequest("valid@email.com");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateSupportAgentInviteCommandResult>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.Not.Empty);
    }
    
    [Test]
    public async Task Post_WithValidData_WithActiveInvite_ShouldReturn201Created()
    {
        var email = "valid@email.com";
        var existingInvite = await CreateSupportAgentInvite(
            email,
            _organizationAdmin.OrganizationId!.IdValue,
            TimeProvider.System);
        
        await AuthenticateAs(_organizationAdmin);
        
        var request = new CreateSupportAgentInviteRequest(email);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateSupportAgentInviteCommandResult>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.Not.Empty);
    }

    [Test]
    public async Task Post_WithAlreadyRegisteredUser_ShouldReturn400BadRequest()
    {
        var supportAgent = await SeedSupportAgent(_organizationAdmin.OrganizationId!.IdValue);

        await AuthenticateAs(_organizationAdmin);
        
        var request = new CreateSupportAgentInviteRequest(DefaultSupportAgentEmail);
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest); 
    }

    [Test]
    public async Task Post_WithInvalidData_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_organizationAdmin);
        
        var request = new CreateSupportAgentInviteRequest("");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);     
    }

    [Test]
    public async Task Post_Unauthenticated_ShouldReturn401Unauthorized()
    {
        var request = new CreateSupportAgentInviteRequest("valid@email.com");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Post_WithNoPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_organizationAdmin);

        await PermissionService.RevokePermissionAsync(_organizationAdmin.Id.IdValue, Permissions.SupportAgentInvites.Create);
        
        var request = new CreateSupportAgentInviteRequest("valid@email.com");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);      
    }
}