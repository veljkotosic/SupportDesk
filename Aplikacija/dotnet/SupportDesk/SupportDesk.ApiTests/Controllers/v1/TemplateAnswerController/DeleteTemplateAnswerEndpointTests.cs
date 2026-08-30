using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.ApiTests.Controllers.v1.TemplateAnswerController;

[TestFixture]
internal sealed class DeleteTemplateAnswerEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/templateAnswer";
    
    [Test]
    public async Task Delete_Authorized_WithValidTemplateAnswerId_ShouldReturn204NoContent()
    {
        var organizationAdmin = await SeedOrganizationAdmin();

        await AuthenticateAs(organizationAdmin);

        var templateAnswer = await CreateTemplateAnswer(organizationAdmin.OrganizationId!.IdValue, "Test", "Test");

        var response = await Client.DeleteAsync($"{EndpointUrl}/{templateAnswer.Id.IdValue}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }
    
    [Test]
    public async Task Delete_Authorized_WithInvalidTemplateAnswerId_ShouldReturn400BadRequest()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);
        
        var response = await Client.DeleteAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);      
    }
    
    [Test]
    public async Task Delete_Authorized_WithAlreadyDeletedTemplateAnswer_ShouldReturn400BadRequest()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);
        
        var templateAnswer = await CreateTemplateAnswer(organizationAdmin.OrganizationId!.IdValue, "Test", "Test");
        templateAnswer.Delete(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var response = await Client.DeleteAsync($"{EndpointUrl}/{templateAnswer.Id.IdValue}");
        
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
    public async Task Delete_WithNoPermissions_ShouldReturn403Forbidden()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        await AuthenticateAs(organizationAdmin);

        await PermissionService.RevokePermissionAsync(organizationAdmin.Id.IdValue, Permissions.TemplateAnswers.Delete);
        
        var response = await Client.DeleteAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);  
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);     
    }
}