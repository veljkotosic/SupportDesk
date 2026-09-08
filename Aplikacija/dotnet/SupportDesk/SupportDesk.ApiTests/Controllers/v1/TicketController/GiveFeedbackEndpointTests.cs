using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;
using SupportDesk.WebApi.Controllers.v1.Ticket.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.TicketController;

[TestFixture]
internal sealed class GiveFeedbackEndpointTests : ApiTestsBase
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
    public async Task Patch_Authorized_WithValidFeedback_ShouldReturn204NoContent()
    {
        await AuthenticateAs(_customer);
        
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();

        var request = new GiveFeedbackRequest(TicketFeedback.Helpful);

        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/giveFeedback", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Patch_Authorized_WithInvalidFeedback_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_customer);
        
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var request = new GiveFeedbackRequest(TicketFeedback.None);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/giveFeedback", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);   
    }
    
    [Test]
    public async Task Patch_Authorized_WithInvalidTicketId_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_customer);
        
        var request = new GiveFeedbackRequest(TicketFeedback.Helpful);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{Guid.NewGuid()}/giveFeedback", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Patch_Authorized_WithFeedbackAlreadyGiven_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_customer);
        
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        _ticket.GiveFeedback(TicketFeedback.Helpful);
        await UnitOfWork.SaveChangesAsync();

        var request = new GiveFeedbackRequest(TicketFeedback.Helpful);

        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/giveFeedback", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Patch_Authorized_WithTicketNotClosed_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_customer);
        
        var request = new GiveFeedbackRequest(TicketFeedback.Helpful);

        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/giveFeedback", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Patch_Unauthenticated_ShouldReturn401Unauthorized()
    {
        var request = new GiveFeedbackRequest(TicketFeedback.Helpful);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/giveFeedback", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);   
    }
    
    [Test]
    public async Task Patch_WithNoPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_customer);
        
        await PermissionService.RevokePermissionAsync(_customer.Id.IdValue, Permissions.Tickets.GiveFeedback);
        
        var request = new GiveFeedbackRequest(TicketFeedback.Helpful);
        
        var response = await Client.PatchAsJsonAsync($"{EndpointUrl}/{_ticket.Id.IdValue}/giveFeedback", request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
    }
}