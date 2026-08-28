using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Models.Auth.Command.Login;
using SupportDesk.Application.Models.Auth.Command.Logout;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Validation.Rules;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Auth.Command.Logout;

[TestFixture]
internal sealed class LogoutTests : IntegrationTestsBase
{
    private User _customer = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _customer = await RegisterCustomer();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_customer.Id.IdValue);
    }
    
    [Test]
    public async Task Handle_WithValidRefreshToken_ShouldLogout()
    {
        var refreshToken = await Login(DefaultCustomerEmail, DefaultCustomerPassword);

        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new LogoutCommand(refreshToken.Value));
        });
        
        var persistedRefreshToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == refreshToken.Value);
        
        Assert.That(persistedRefreshToken, Is.Null);       
    }
    
    [Test]
    public async Task Handle_WithOtherRefreshToken_ShouldBreakUserCanOnlyLogoutHimselfRule()
    {
        var otherCustomer = await RegisterCustomer("other@other.com", "Other Customer");

        var refreshToken = await Login("other@other.com", DefaultCustomerPassword);
            
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new LogoutCommand(refreshToken.Value));
        });
        
        AssertUtility.AssertHasBrokenExactRule<UserCanOnlyLogoutHimselfRule>(exception);
    }
}