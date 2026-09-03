using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Models.Auth.Command.LogoutAll;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.IntegrationTests.Application.Models.Auth.Command;

[TestFixture]
internal sealed class LogoutAllTests : IntegrationTestsBase
{
    private User _user = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _user = await RegisterCustomer();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_user.Id.IdValue);
    }

    [Test]
    public async Task Handle_WithMultipleLogins_ShouldLogoutAll()
    {
        for (int i = 0; i < 5; i++)
        {
            await Login(DefaultCustomerEmail, DefaultCustomerPassword);
        }
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new LogoutAllCommand());
        });
        
        var persistedRefreshTokensExist = await DbContext.RefreshTokens
            .AsNoTracking()
            .AnyAsync(r => r.UserId == _user.Id.IdValue);
        
        Assert.That(persistedRefreshTokensExist, Is.False);       
    }
}