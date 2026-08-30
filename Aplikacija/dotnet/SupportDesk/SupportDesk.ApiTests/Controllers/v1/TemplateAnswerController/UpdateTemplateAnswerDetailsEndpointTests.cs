using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.WebApi.Controllers.v1.TemplateAnswer.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.TemplateAnswerController;

[TestFixture]
internal sealed class UpdateTemplateAnswerDetailsEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/templateAnswer";
    
    [TestCase("Test 2", "Test 2")]
    [TestCase(null, "Test 2")]
    [TestCase("Test 2", null)]
    public async Task Patch_Authorized_WithValidData_ShouldReturn204NoContent(string? newTitle, string? newText)
    {
        var user = await SeedOrganizationAdmin();

        await AuthenticateAs(user);
        
        var templateAnswer = await CreateTemplateAnswer(user.OrganizationId!.IdValue, "Test", "Test");
        
        var request = new UpdateTemplateAnswerDetailsRequest(newTitle, newText);

        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{templateAnswer.Id.IdValue}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }
    
    [Test]
    public async Task Patch_WithInvalidData_ShouldReturn400BadRequest()
    {
        var user = await SeedOrganizationAdmin();

        await AuthenticateAs(user);
        
        var request = new UpdateTemplateAnswerDetailsRequest(null, null);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_WithInvalidTemplateAnswerId_ShouldReturn400BadRequest()
    {
        var user = await SeedOrganizationAdmin();

        await AuthenticateAs(user);
        
        var request = new UpdateTemplateAnswerDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_Unauthorized_ShouldReturn401Unauthorized()
    {
        var request = new UpdateTemplateAnswerDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task Patch_WithNoPermissions_ShouldReturn403Forbidden()
    {
        var user = await SeedOrganizationAdmin();
        
        await AuthenticateAs(user);
        
        await PermissionService.RevokePermissionAsync(user.Id.IdValue, Permissions.TemplateAnswers.Update);
        
        var request = new UpdateTemplateAnswerDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);      
    }
}