using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Models.Auth.Command.Login;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User.Validation;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Auth.Command.Login;

[TestFixture]
internal sealed class LoginTests : IntegrationTestsBase
{
    [Test]
    public async Task Handle_AsCustomer_WithValidInput_ShouldReturnTokens()
    {
        var customer = await RegisterCustomer();
        
        LoginCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new LoginCommand(DefaultCustomerEmail, DefaultCustomerPassword));
        });
        
        Assert.That(result, Is.Not.Null);

        var persistedRefreshToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == result.RefreshToken.Value);
        
        Assert.That(persistedRefreshToken, Is.Not.Null);
        Assert.That(persistedRefreshToken.Token, Is.EqualTo(result.RefreshToken.Value));
        Assert.That(persistedRefreshToken.UserId, Is.EqualTo(customer.Id.IdValue));
    }
    
    [Test]
    public async Task Handle_AsSupportAgent_WithValidInput_ShouldReturnTokens()
    {
        var supportAgent = await RegisterDefaultSupportAgent();
        
        LoginCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new LoginCommand(DefaultSupportAgentEmail, DefaultSupportAgentPassword));
        });
        
        Assert.That(result, Is.Not.Null);
        
        var persistedRefreshToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == result.RefreshToken.Value);
        
        Assert.That(persistedRefreshToken, Is.Not.Null);
        Assert.That(persistedRefreshToken.Token, Is.EqualTo(result.RefreshToken.Value));
        Assert.That(persistedRefreshToken.UserId, Is.EqualTo(supportAgent.Id.IdValue));
    }
    
    [Test]
    public async Task Handle_AsOrganizationAdmin_WithValidInput_ShouldReturnTokens()
    {
        var organizationAdmin = await RegisterOrganizationAdmin();
        
        LoginCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new LoginCommand(DefaultOrganizationAdminEmail, DefaultOrganizationAdminPassword));
        });
        
        Assert.That(result, Is.Not.Null);
        
        var persistedRefreshToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == result.RefreshToken.Value);
        
        Assert.That(persistedRefreshToken, Is.Not.Null);
        Assert.That(persistedRefreshToken.Token, Is.EqualTo(result.RefreshToken.Value));
        Assert.That(persistedRefreshToken.UserId, Is.EqualTo(organizationAdmin.Id.IdValue));
    }

    [TestCase(DefaultCustomerEmail, "")]
    [TestCase("", DefaultCustomerPassword)]
    [TestCase("", "")]
    public async Task Handle_WithInvalidInput_ShouldThrowValidationException(string email, string password)
    {
        _ = await RegisterCustomer();

        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new LoginCommand(email, password));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, UserErrors.InvalidCredentials());
    }
}