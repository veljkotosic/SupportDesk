using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.ApiTests.Controllers.v1.CategoryController;

[TestFixture]
internal sealed class DeleteCategoryEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/category";
    
    [Test]
    public async Task Delete_Authorized_WithValidCategoryId_ShouldReturn204NoContent()
    {
        var organizationAdmin = await SeedOrganizationAdmin();

        await AuthenticateAs(organizationAdmin);

        var category = await CreateCategory(organizationAdmin.OrganizationId!.IdValue, "Test", "Test");

        var response = await Client.DeleteAsync($"{EndpointUrl}/{category.Id.IdValue}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }
    
    [Test]
    public async Task Delete_Authorized_WithInvalidCategoryId_ShouldReturn400BadRequest()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);
        
        var response = await Client.DeleteAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);   
    }
    
    [Test]
    public async Task Delete_Authorized_WithAlreadyDeletedCategory_ShouldReturn400BadRequest()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);
        
        var category = await CreateCategory(organizationAdmin.OrganizationId!.IdValue, "Test", "Test");
        category.Delete(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var response = await Client.DeleteAsync($"{EndpointUrl}/{category.Id.IdValue}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);   
    }
    
    [Test]
    public async Task Delete_Unauthorized_ShouldReturn401Unauthorized()
    {
        var response = await Client.DeleteAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);   
    }
    
    [Test]
    public async Task Delete_WithNoPermissions_ShouldReturn403Forbidden()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);

        await PermissionService.RevokePermissionAsync(organizationAdmin.Id.IdValue, Permissions.Categories.Delete);
        
        var response = await Client.DeleteAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);  
    }
}