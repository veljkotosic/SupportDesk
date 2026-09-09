using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.SupportAgentInviteController;

[TestFixture]
internal sealed class RevokeSupportAgentInviteEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/supportAgentInvite";
    
    private User _organizationAdmin;
    
    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await SeedOrganizationAdmin();
    }

    [Test]
    public async Task Delete_WithValidInviteId_ShouldReturn204NoContent()
    {
        await AuthenticateAs(_organizationAdmin);
        
        var invite = await CreateSupportAgentInvite(
            "valid@email.com",
            _organizationAdmin.OrganizationId!.IdValue,
            TimeProvider.System);

        var response = await Client.DeleteAsync($"{EndpointUrl}/{invite.Id.IdValue}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_WithInvalidInviteId_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_organizationAdmin);
        
        var response = await Client.DeleteAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Delete_Unauthorized_ShouldReturn401Unauthorized()
    {
        var response = await Client.DeleteAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task Delete_WithoutPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_organizationAdmin);

        await PermissionService.RevokePermissionAsync(_organizationAdmin.Id.IdValue, Permissions.SupportAgentInvites.Revoke);
        
        var response = await Client.DeleteAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);     
    }
}