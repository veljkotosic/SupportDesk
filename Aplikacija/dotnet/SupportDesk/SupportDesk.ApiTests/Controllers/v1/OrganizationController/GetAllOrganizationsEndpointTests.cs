using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.OrganizationController;

[TestFixture]
internal sealed class GetAllOrganizationsEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/organization/all";
    
    private User _customer;
    
    [SetUp]
    public async Task Setup()
    {
        _customer = await SeedCustomerAsync();
    }
    
    [Test]
    public async Task Get_Authorized_ShouldReturn200OK()
    {
        await AuthenticateAs(_customer);
        
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
        await AuthenticateAs(_customer);
        
        await PermissionService.RevokePermissionAsync(
            _customer.Id.IdValue,
            Permissions.Organizations.ViewAll);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);     
    }
}