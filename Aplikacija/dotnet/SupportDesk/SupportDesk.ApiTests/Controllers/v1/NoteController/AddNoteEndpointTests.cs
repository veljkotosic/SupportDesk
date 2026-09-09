using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Notes.Command.AddNote;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;
using SupportDesk.WebApi.Controllers.v1.Note.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.NoteController;

[TestFixture]
internal sealed class AddNoteEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/note";
    
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
    public async Task Post_Authorized_AsSupportAgent_WithValidData_ShouldReturn201Created()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        await AuthenticateAs(_supportAgent);
        
        var request = new AddNoteRequest(_ticket.Id.IdValue, "Test Note");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<AddNoteCommandResult>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NoteId, Is.Not.EqualTo(Guid.Empty));   
    }
    
    [Test]
    public async Task Post_Authorized_AsOrganizationAdmin_WithValidData_ShouldReturn201Created()
    {
        await AuthenticateAs(_organizationAdmin);
        
        var request = new AddNoteRequest(_ticket.Id.IdValue, "Test Note");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<AddNoteCommandResult>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NoteId, Is.Not.EqualTo(Guid.Empty));   
    }
    
    [Test]
    public async Task Post_WithInvalidData_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_organizationAdmin);
        
        var request = new AddNoteRequest(Guid.NewGuid(), "");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);  
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);  
    }

    [Test]
    public async Task Post_Unauthorized_ShouldReturn401Unauthorized()
    {
        var request = new AddNoteRequest(_ticket.Id.IdValue, "Test Note");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);   
    }

    [Test]
    public async Task Post_WithNoPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_organizationAdmin);

        await PermissionService.RevokePermissionAsync(_organizationAdmin.Id.IdValue, Permissions.Notes.Add);
        
        var request = new AddNoteRequest(_ticket.Id.IdValue, "Test Note");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden); 
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);
    }
}