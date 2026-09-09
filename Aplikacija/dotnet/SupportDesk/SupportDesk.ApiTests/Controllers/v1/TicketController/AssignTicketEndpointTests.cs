using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.TicketController;

[TestFixture]
internal sealed class AssignTicketEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = $"api/v1/ticket";

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
    public async Task Patch_Authorized_ShouldReturn204NoContent()
    {
        await AuthenticateAs(_supportAgent);

        var response = await Client.PatchAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/assign", null);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Patch_WithTicketAlreadyAssigned_ShouldReturn400BadRequest()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        await AuthenticateAs(_supportAgent);

        var response = await Client.PatchAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/assign", null);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_WithTicketAlreadyClosed_ShouldReturn400BadRequest()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        await AuthenticateAs(_supportAgent);

        var response = await Client.PatchAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/assign", null);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_Unauthenticated_ShouldReturn401Unauthorized()
    {
        var response = await Client.PatchAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/assign", null);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Patch_WithNoPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_supportAgent);

        await PermissionService.RevokePermissionAsync(_supportAgent.Id.IdValue, Permissions.Tickets.Assign);

        var response = await Client.PatchAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/assign", null);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);
    }
}