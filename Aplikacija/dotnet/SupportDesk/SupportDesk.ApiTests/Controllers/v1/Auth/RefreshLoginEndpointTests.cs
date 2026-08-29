using System.Net;
using Microsoft.EntityFrameworkCore;
using SupportDesk.ApiTests.Utility;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.ApiTests.Controllers.v1.Auth;

[TestFixture]
internal sealed class RefreshLoginEndpointTests : ApiTestsBase
{
    private const string EndpointUrl = "api/v1/Auth/refreshLogin";

    private User _user = null!;

    [SetUp]
    public async Task Setup()
    {
        _user = await SeedCustomerAsync();
    }

    [Test]
    public async Task Post_WithValidRefreshToken_ShouldReturn204NoContent()
    {
        await AuthenticateAs(_user);

        var response = await Client.PostAsync(EndpointUrl, null);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.NoContent);
        AssertResponse.HasAuthCookiesSet(response);
    }

    [Test]
    public async Task Post_WithInvalidRefreshToken_ShouldReturn401Unauthorized()
    {
        var response = await Client.PostAsync(EndpointUrl, null);
                
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Post_WithExpiredRefreshToken_ShouldReturn401Unauthorized()
    {
        await  AuthenticateAs(_user);

        var refreshToken = await DbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.UserId == _user.Id.IdValue);
        
        refreshToken!.ExpiresAt = DateTime.UtcNow.AddYears(-1);
        await DbContext.SaveChangesAsync();
        
        var response = await Client.PostAsync(EndpointUrl, null);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Post_WithOtherUsersRefreshToken_ShouldBreakUserCanOnlyRefreshHisLoginRule()
    {
        var otherUser = await SeedOrganizationAdmin();
        
        var tokenProvider = GetRequiredService<ITokenProvider>();
        var refreshTokenManager = GetRequiredService<IRefreshTokenManager>();
        
        var otherUserRefreshTokenValue = tokenProvider.GenerateRefreshToken();
        
        _ = await refreshTokenManager.AddAsync(otherUserRefreshTokenValue, otherUser.Id.IdValue, otherUser.Role, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var userAccessToken = tokenProvider.GenerateAccessToken(_user);

        var request = new HttpRequestMessage(HttpMethod.Post, EndpointUrl);
        request.Headers.Add("Cookie", $"accessToken={userAccessToken.Value}");
        request.Headers.Add("Cookie", $"refreshToken={otherUserRefreshTokenValue}");
        
        var response = await Client.SendAsync(request);
        
        AssertResponse.HasStatusCode(response, HttpStatusCode.BadRequest);       
        await AssertProblemDetails.ExistsWithStatus(response, HttpStatusCode.BadRequest);
    }
}