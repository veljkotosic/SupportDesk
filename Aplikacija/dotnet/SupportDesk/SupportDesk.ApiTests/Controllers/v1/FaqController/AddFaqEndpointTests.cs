using System.Net;
using System.Net.Http.Json;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Faqs.Command.AddFaq;
using SupportDesk.Domain.Models.User;
using SupportDesk.WebApi.Controllers.v1.Faq.Requests;

namespace SupportDesk.ApiTests.Controllers.v1.FaqController;

[TestFixture]
internal sealed class AddFaqEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/faq";

    private User _user = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _user = await SeedOrganizationAdmin();
    }

    [Test]
    public async Task Post_WithValidData_ShouldReturn201Created()
    {
        await AuthenticateAs(_user);
        
        var request = new AddFaqRequest("Test Question", "Test Answer");

        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<AddFaqCommandResult>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FaqId, Is.Not.EqualTo(Guid.Empty));
    }
    
    [Test]
    public async Task Post_WithInvalidData_ShouldReturn400BadRequest()
    {
        await AuthenticateAs(_user);
        
        var request = new AddFaqRequest("", "");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Post_Unauthorized_ShouldReturn401Unauthorized()
    {
        var request = new AddFaqRequest("Test Question", "Test Answer");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Post_WithNoPermissions_ShouldReturn403Forbidden()
    {
        await AuthenticateAs(_user);

        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.Faqs.Add);
        
        var request = new AddFaqRequest("Test Question", "Test Answer");
        
        var response = await Client.PostAsJsonAsync(EndpointUrl, request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Forbidden);
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.Forbidden);      
    }
}