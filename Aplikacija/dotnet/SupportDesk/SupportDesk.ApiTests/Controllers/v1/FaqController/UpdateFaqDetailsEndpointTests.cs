using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.WebApi.Controllers.v1.Faq.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.FaqController;

[TestFixture]
internal sealed class UpdateFaqDetailsEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/faq";
    
    [TestCase("Test 2", "Test 2")]
    [TestCase(null, "Test 2")]
    [TestCase("Test 2", null)]
    public async Task Patch_Authorized_WithValidData_ShouldReturn204NoContent(string? newQuestion, string? newAnswer)
    {
        var user = await SeedOrganizationAdmin();

        await AuthenticateAs(user);
        
        var faq = await CreateFaq(user.OrganizationId!.IdValue, "Test", "Test");
        
        var request = new UpdateFaqDetailsRequest(newQuestion, newAnswer);

        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{faq.Id.IdValue}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }
    
    [Test]
    public async Task Patch_WithInvalidData_ShouldReturn400BadRequest()
    {
        var user = await SeedOrganizationAdmin();

        await AuthenticateAs(user);
        
        var request = new UpdateFaqDetailsRequest(null, null);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_WithInvalidFaqId_ShouldReturn400BadRequest()
    {
        var user = await SeedOrganizationAdmin();

        await AuthenticateAs(user);
        
        var request = new UpdateFaqDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_Unauthorized_ShouldReturn401Unauthorized()
    {
        var request = new UpdateFaqDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task Patch_WithNoPermissions_ShouldReturn403Forbidden()
    {
        var user = await SeedOrganizationAdmin();
        
        await AuthenticateAs(user);
        
        await PermissionService.RevokePermissionAsync(user.Id.IdValue, Permissions.Faqs.Update);
        
        var request = new UpdateFaqDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);      
    }
}