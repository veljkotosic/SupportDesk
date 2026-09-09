using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.TicketController;

[TestFixture]
internal sealed class GetTicketEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/ticket";
    
    private User _organizationAdmin = null!;
    private User _supportAgent = null!;
    private User _customer = null!;
    private Category _category = null!;
    private Ticket _ticket = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await SeedOrganizationAdmin();
        _supportAgent = await SeedSupportAgent(_organizationAdmin.OrganizationId!.IdValue);
        _category = await CreateCategory(_organizationAdmin.OrganizationId!.IdValue, "Test", "Test", TimeProvider.System);
        
        _customer = await SeedCustomerAsync();
        
        _ticket = await CreateTicket(
            _organizationAdmin.OrganizationId.IdValue, 
            _category.Id.IdValue,
            _customer.Id.IdValue,
            TicketPriority.Medium,
            "Test Ticket",
            "Test Message",
            TimeProvider.System);
    }
    
    [Test]
    public async Task Get_WithValidTicketId_ShouldReturn200OK()
    {
        await AuthenticateAs(_customer);
        
        var response = await Client.GetAsync($"{EndpointUrl}/{_ticket.Id.IdValue}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.OK);
    }
    
    [Test]
    public async Task Get_WithInvalidTicketId_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_customer);
        
        var response = await Client.GetAsync($"{EndpointUrl}/{Guid.NewGuid()}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);  
    }
    
    [Test]
    public async Task Get_Unauthenticated_ShouldReturn401Unauthorized()
    {
        var response = await Client.GetAsync($"{EndpointUrl}/{_ticket.Id.IdValue}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Get_WithoutPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_customer);
        
        await PermissionService.RevokePermissionAsync(_customer.Id.IdValue, Permissions.Tickets.Get);
        
        var response = await Client.GetAsync($"{EndpointUrl}/{_ticket.Id.IdValue}");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden); 
    }
}