using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.OrganizationController;

[TestFixture]
internal sealed class GetOrganizationKnowledgeBaseEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/organization/knowledgeBase";
    
    private User _organizationAdmin;
    
    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await SeedOrganizationAdmin();
    }
    
    [Test]
    public async Task Get_Authorized_ShouldReturn200OK()
    {
        await AuthenticateAs(_organizationAdmin);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.OK);
    }

    [Test]
    public async Task Get_Unauthorized_ShouldReturn401Unauthorized()
    {
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task Get_WithNoPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_organizationAdmin);
        
        await PermissionService.RevokePermissionAsync(
            _organizationAdmin.Id.IdValue,
            Permissions.Organizations.ViewKnowledgeBase);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);     
    }
}