using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.WebApi.Controllers.v1.Ticket.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.TicketController;

[TestFixture]
internal sealed class OpenTicketEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/ticket";
    
    [Test]
    public async Task Post_Authorized_WithValidData_ShouldReturn201Created()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        var category = await CreateCategory(organizationAdmin.OrganizationId!.IdValue, "Test", "Test");

        var customer = await SeedCustomerAsync();
        await AuthenticateAs(customer);
        
        var request = new OpenTicketRequest(
            category.OrganizationId.IdValue,
            category.Id.IdValue,
            TicketPriority.Medium,
            "Test",
            "Test");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Created);
    }

    [TestCase("", "")]
    [TestCase("Test", "")]
    [TestCase("", "Test")]
    public async Task Post_Authorized_WithInvalidData_ShouldReturn400BadRequest(string invalidSubject, string invalidMessage)
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        var category = await CreateCategory(organizationAdmin.OrganizationId!.IdValue, "Test", "Test");

        var customer = await SeedCustomerAsync();
        await AuthenticateAs(customer);
        
        var request = new OpenTicketRequest(
            category.OrganizationId.IdValue,
            category.Id.IdValue,
            TicketPriority.Medium,
            invalidSubject,
            invalidMessage);

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);    
    }
    
    [Test]
    public async Task Post_Authorized_WithInvalidCategory_ShouldReturn400BadRequest()
    {
        var organizationAdmin = await SeedOrganizationAdmin();
        
        var customer = await SeedCustomerAsync();
        await AuthenticateAs(customer);
        
        var request = new OpenTicketRequest(
            organizationAdmin.OrganizationId!.IdValue,
            Guid.NewGuid(), 
            TicketPriority.Medium,
            "Test",
            "Test");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);    
    }
    
    [Test]
    public async Task Post_Authorized_WithInvalidOrganization_ShouldReturn400BadRequest()
    {
        var customer = await SeedCustomerAsync();
        await AuthenticateAs(customer);
        
        var request = new OpenTicketRequest(
            OrganizationId: Guid.NewGuid(),
            Guid.NewGuid(), 
            TicketPriority.Medium,
            "Test",
            "Test");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);    
    }

    [Test]
    public async Task Post_Unauthorized_ShouldReturn401Unauthorized()
    {
        var request = new OpenTicketRequest(Guid.NewGuid(), Guid.NewGuid(), TicketPriority.Medium, "Test", "Test");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task Post_WithNoPermissions_ShouldReturn403Forbidden()
    {
        var user = await SeedCustomerAsync();
        await AuthenticateAs(user);

        await PermissionService.RevokePermissionAsync(user.Id.IdValue, Permissions.Tickets.Open);
        
        var request = new OpenTicketRequest(Guid.NewGuid(), Guid.NewGuid(), TicketPriority.Medium, "Test", "Test");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden); 
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);     
    }
}