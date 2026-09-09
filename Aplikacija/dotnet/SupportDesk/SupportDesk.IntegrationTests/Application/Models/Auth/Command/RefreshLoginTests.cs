using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Models.Auth.Command.RefreshLogin;
using SupportDesk.Domain.Models.User;
using SupportDesk.Infrastructure.Persistence.RefreshToken;

namespace SupportDesk.IntegrationTests.Application.Models.Auth.Command;

[TestFixture]
internal sealed class RefreshLoginTests : IntegrationTestsBase
{
    private User _customer = null!;

    [SetUp]
    public async Task Setup()
    {
        _customer = await RegisterCustomer();

        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_customer.Id.IdValue);
    }

    [Test]
    public async Task Handle_WithValidRefreshToken_ShouldGenerateNewRefreshTokenAndRevokeOldOne()
    {
        var oldRefreshToken = await Login(DefaultCustomerEmail, DefaultCustomerPassword);
        RefreshLoginCommandResult? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new RefreshLoginCommand(oldRefreshToken.Value));
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AccessToken, Is.Not.Null);
        Assert.That(result.AccessToken.Value, Is.Not.Null.And.Not.Empty);
        Assert.That(result.RefreshToken, Is.Not.Null);
        Assert.That(result.RefreshToken.Value, Is.Not.EqualTo(oldRefreshToken.Value));
        Assert.That(result.RefreshToken.UserId, Is.EqualTo(_customer.Id.IdValue));

        var persistedOldToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == oldRefreshToken.Value);
        
        Assert.That(persistedOldToken, Is.Null);

        var persistedNewToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == result.RefreshToken.Value);
        
        Assert.That(persistedNewToken, Is.Not.Null);
        Assert.That(persistedNewToken!.UserId, Is.EqualTo(_customer.Id.IdValue));
    }

    [Test]
    public async Task Handle_WithExpiredRefreshToken_ShouldThrowRefreshTokenException_TokenExpired()
    {
        _ = await Login(DefaultCustomerEmail, DefaultCustomerPassword); 
        
        var token = await DbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.UserId == _customer.Id.IdValue);
        
        token!.ExpiresAt = DateTime.UtcNow.AddYears(-1);
        
        await DbContext.SaveChangesAsync();

        var exception = Assert.ThrowsAsync<RefreshTokenException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new RefreshLoginCommand(token.Token));
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.ErrorCode, Is.EqualTo(RefreshTokenException.TokenExpiredCode));
    }

    [Test]
    public async Task Handle_WithFakedRefreshToken_ShouldThrowRefreshTokenException_InvalidToken()
    {
        var fakeTokenValue = "FakeNonExistentRefreshToken12345!";

        var exception = Assert.ThrowsAsync<RefreshTokenException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new RefreshLoginCommand(fakeTokenValue));
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.ErrorCode, Is.EqualTo(RefreshTokenException.InvalidTokenCode));
    }
}
