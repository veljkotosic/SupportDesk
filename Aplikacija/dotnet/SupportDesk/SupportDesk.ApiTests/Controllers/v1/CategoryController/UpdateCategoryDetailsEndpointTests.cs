using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.User;
using SupportDesk.WebApi.Controllers.v1.Category.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.CategoryController;

[TestFixture]
internal sealed class UpdateCategoryDetailsEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/category";

    private User _user = null!;

    [SetUp]
    public async Task Setup()
    {
        _user = await SeedOrganizationAdmin();
    }
    
    [TestCase("Test 2", "Test 2")]
    [TestCase(null, "Test 2")]
    [TestCase("Test 2", null)]
    public async Task Patch_WithValidData_ShouldReturn204NoContent(string? newName, string? newDescription)
    {
        await AuthenticateAs(_user);
        
        var category = await CreateCategory(_user.OrganizationId!.IdValue, "Test", "Test");

        var request = new UpdateCategoryDetailsRequest(newName, newDescription);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{category.Id.IdValue}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }
    
    [Test]
    public async Task Patch_WithSameCategoriesName_ShouldReturn204NoContent()
    {
        await AuthenticateAs(_user);
        
        var category = await CreateCategory(_user.OrganizationId!.IdValue, "Test", "Test");
        
        var request = new UpdateCategoryDetailsRequest("Test", null);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{category.Id.IdValue}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Patch_WithOtherCategoriesName_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_user);
        
        var category = await CreateCategory(_user.OrganizationId!.IdValue, "Test", "Test");
        _ = await CreateCategory(_user.OrganizationId!.IdValue, "Test 2", "Test 2");
        
        var request = new UpdateCategoryDetailsRequest("Test 2", null);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{category.Id.IdValue}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_WithInvalidCategoryId_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_user);
    
        var request = new UpdateCategoryDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_WithInvalidData_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_user);
        
        var category = await CreateCategory(_user.OrganizationId!.IdValue, "Test", "Test");
    
        var request = new UpdateCategoryDetailsRequest(null, null);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Patch_Unauthorized_ShouldReturn401Unauthorized()
    {
        var request = new UpdateCategoryDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Patch_WithNoPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_user);

        await PermissionService.RevokePermissionAsync(_user.Id!.IdValue, Permissions.Categories.Update);
        
        var request = new UpdateCategoryDetailsRequest("Test 2", "Test 2");
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);
    }
}