using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.TicketController;

[TestFixture]
internal sealed class GetCustomerTicketsEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/ticket/customer";
    
    private User _customer = null!;

    [SetUp]
    public async Task Setup()
    {
        _customer = await SeedCustomerAsync();
    }

    [Test]
    public async Task Get_Authenticated_ShouldReturn200OK()
    {
        await AuthenticateAs(_customer);
        
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
        await AuthenticateAs(_customer);

        await PermissionService.RevokePermissionAsync(_customer.Id.IdValue, Permissions.Tickets.GetCustomerTickets);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);    
    }
}