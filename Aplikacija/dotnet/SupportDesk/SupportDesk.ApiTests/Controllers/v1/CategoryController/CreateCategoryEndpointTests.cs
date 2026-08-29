using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Categories.Command.AddCategory;
using SupportDesk.WebApi.Controllers.v1.Category.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.CategoryController;

[TestFixture]
internal sealed class CreateCategoryEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/category";
    
    [Test]
    public async Task Post_Authorized_WithValidData_ShouldReturnCategoryId()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);
        
        var request = new AddCategoryRequest("Test Category", "Test Description");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<AddCategoryCommandResult>(JsonOptions);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CategoryId, Is.Not.EqualTo(Guid.Empty));
    }
    
    [TestCase("", "Test Description")]
    [TestCase("Test Category", "😊")]
    public async Task Post_WithInvalidData_ShouldReturn400BadRequest_WithProblemDetails(string name, string description)
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);
        
        var request = new AddCategoryRequest(name, description);
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);       
    }

    [Test]
    public async Task Post_Unauthorized_ShouldReturn401Unauthorized()
    {
        var response = await Client.PostAsJsonAsync(EndpointUrl, new AddCategoryRequest("Test Category", "Test Description"));
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Post_WithNoPermissions_ShouldReturn403Forbidden()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);

        await PermissionService.RevokePermissionAsync(organizationAdmin.Id.IdValue, Permissions.Categories.Add);
        
        var request = new AddCategoryRequest("Test Category", "Test Description");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);       
    }
}