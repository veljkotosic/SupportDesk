using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.TicketController;

[TestFixture]
internal sealed class GetOrganizationTicketsEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/ticket/organization";

    private User _organizationAdmin;
    private User _supportAgent;

    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await SeedOrganizationAdmin();
        _supportAgent = await SeedSupportAgent(_organizationAdmin.OrganizationId!.IdValue);
    }
    
    [Test]
    public async Task Get_Authenticated_AsOrganizationAdmin_ShouldReturn200OK()
    {
        await AuthenticateAs(_supportAgent);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.OK);
    }
    
    [Test]
    public async Task Get_Authenticated_AsSupportAgent_ShouldReturn200OK()
    {
        await AuthenticateAs(_organizationAdmin);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.OK);
    }
    
    [Test]
    public async Task Get_Unauthenticated_ShouldReturn401Unauthorized()
    {
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Get_WithoutPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_organizationAdmin);

        await PermissionService.RevokePermissionAsync(_organizationAdmin.Id.IdValue, Permissions.Tickets.GetOrganizationTickets);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);    
    }
}