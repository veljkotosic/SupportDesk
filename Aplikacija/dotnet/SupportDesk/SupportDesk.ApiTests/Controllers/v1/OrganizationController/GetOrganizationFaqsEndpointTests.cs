using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.OrganizationController;

[TestFixture]
internal sealed class GetOrganizationFaqsEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/organization";
    
    private User _organizationAdmin;
    private User _customer;

    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await SeedOrganizationAdmin();
        _customer = await SeedCustomerAsync();
    }

    [Test]
    public async Task Get_WithValidOrganizationId_ShouldReturn200OK()
    {
        await AuthenticateAs(_customer);

        var response = await Client.GetAsync($"{EndpointUrl}/{_organizationAdmin.OrganizationId!.IdValue}/faqs");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.OK);
    }

    [Test]
    public async Task Get_WithInvalidOrganizationId_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_customer);
        
        var response = await Client.GetAsync($"{EndpointUrl}/{Guid.NewGuid()}/faqs");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);      
    }

    [Test]
    public async Task Get_Unauthenticated_ShouldReturn401Unauthorized()
    {
        var response = await Client.GetAsync($"{EndpointUrl}/{_organizationAdmin.OrganizationId!.IdValue}/faqs");
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }
}