using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.Auth;

[TestFixture]
internal sealed class LogoutAllEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/auth/logoutAll";
    
    private User _user = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _user = await SeedCustomerAsync();
    }

    [Test]
    public async Task Delete_WithValidLogin_ShouldReturn204NoContent_AndDeleteAuthCookies()
    {
        await AuthenticateAs(_user);

        var response = await Client.DeleteAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
        AssertResponse.HasAuthCookiesDeleted(response);   
    }
    
    [Test]
    public async Task Delete_WithoutValidLogin_ShouldReturn401Unauthorized()
    {
        var response = await Client.DeleteAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }
}