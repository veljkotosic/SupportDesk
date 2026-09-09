using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Models.Auth.Query.GetMe;

namespace SupportDesk.ApiTests.Controllers.v1.AuthController;

[TestFixture]
internal sealed class GetMeEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/auth/me";
    
    [Test]
    public async Task Get_AsCustomer_ShouldReturnCustomer()
    {
        var customer = await SeedCustomerAsync();

        await AuthenticateAs(customer);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetMeQueryResult>(JsonOptions);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(customer.Id.IdValue));
        Assert.That(result.Email, Is.EqualTo(customer.Email.EmailValue));
        Assert.That(result.UserName, Is.EqualTo(customer.UserName.UserNameValue));
        Assert.That(result.Role, Is.EqualTo(customer.Role));
    }
    
    [Test]
    public async Task Get_AsSupportAgent_ShouldReturnSupportAgent()
    {
        var organizationAdmin = await SeedOrganizationAdmin();

        var supportAgent = await SeedSupportAgent(organizationAdmin.OrganizationId!.IdValue);

        await AuthenticateAs(supportAgent);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetMeQueryResult>(JsonOptions);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(supportAgent.Id.IdValue));
        Assert.That(result.Email, Is.EqualTo(supportAgent.Email.EmailValue));
        Assert.That(result.UserName, Is.EqualTo(supportAgent.UserName.UserNameValue));
        Assert.That(result.Role, Is.EqualTo(supportAgent.Role));
    }
    
    [Test]
    public async Task Get_AsOrganizationAdmin_ShouldReturnOrganizationAdmin()
    {
        var organizationAdmin = await SeedOrganizationAdmin();

        await AuthenticateAs(organizationAdmin);
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetMeQueryResult>(JsonOptions);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(organizationAdmin.Id.IdValue));
        Assert.That(result.Email, Is.EqualTo(organizationAdmin.Email.EmailValue));
        Assert.That(result.UserName, Is.EqualTo(organizationAdmin.UserName.UserNameValue));
        Assert.That(result.Role, Is.EqualTo(organizationAdmin.Role));
    }
    
    [Test]
    public async Task Get_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        var response = await Client.GetAsync(EndpointUrl);

        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task Get_WithInvalidToken_ShouldReturn400BadRequest()
    {
        var user = await SeedCustomerAsync();
        
        await AuthenticateAs(user);

        await DbContext.DomainUsers
            .Where(u => u.Id == user.Id)
            .ExecuteDeleteAsync();
        
        var response = await Client.GetAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
}