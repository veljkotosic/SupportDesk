using System.Net;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.Auth;

[TestFixture]
internal sealed class LogoutEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "/api/v1/auth/logout";

    private User _user = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _user = await SeedCustomerAsync();
    }

    [Test]
    public async Task Delete_WithValidCookies_ShouldReturn204NoContent_AndDeleteAuthCookies()
    {
        await AuthenticateAs(_user);
        
        var response = await Client.DeleteAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
        AssertResponse.HasAuthCookiesDeleted(response);
    }

    [Test]
    public async Task Delete_WithoutValidCookies_ShouldReturn401Unauthorized()
    {
        var response = await Client.DeleteAsync(EndpointUrl);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Delete_WithoutRefreshTokenCookie_ShouldReturn401Unauthorized()
    {
        var tokenProvider = GetRequiredService<ITokenProvider>();
        var accessToken = tokenProvider.GenerateAccessToken(_user);

        var request = new HttpRequestMessage(HttpMethod.Delete, EndpointUrl);
        request.Headers.Add("Cookie", $"accessToken={accessToken.Value}");
        
        var response = await Client.SendAsync(request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Delete_WithOtherUsersRefreshTokenCookie_ShouldReturn400BadRequest()
    {
        var otherUser = await SeedOrganizationAdmin();
        
        var tokenProvider = GetRequiredService<ITokenProvider>();
        var refreshTokenManager = GetRequiredService<IRefreshTokenManager>();
        
        var otherUserRefreshTokenValue = tokenProvider.GenerateRefreshToken();
        
        _ = await refreshTokenManager.AddAsync(otherUserRefreshTokenValue, otherUser.Id.IdValue, otherUser.Role, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var userAccessToken = tokenProvider.GenerateAccessToken(_user);

        var request = new HttpRequestMessage(HttpMethod.Delete, EndpointUrl);
        request.Headers.Add("Cookie", $"accessToken={userAccessToken.Value}");
        request.Headers.Add("Cookie", $"refreshToken={otherUserRefreshTokenValue}");
        
        var response = await Client.SendAsync(request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);       
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
}