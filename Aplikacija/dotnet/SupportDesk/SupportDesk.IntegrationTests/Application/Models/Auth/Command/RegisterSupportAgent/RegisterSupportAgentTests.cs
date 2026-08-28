using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using SupportDesk.Application.Models.Auth.Command.RegisterSupportAgent;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.Enums;
using SupportDesk.Domain.Models.SupportAgentInvite.Options;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Auth.Command.RegisterSupportAgent;

[TestFixture]
internal sealed class RegisterSupportAgentTests : IntegrationTestsBase
{
    private User _organizationAdmin = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await RegisterOrganizationAdmin();
    }
    
    [Test]
    public async Task Handle_WithValidData_ShouldRegisterSupportAgent()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _organizationAdmin.OrganizationId!.IdValue, TimeProvider.System);
        
        RegisterSupportAgentCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new RegisterSupportAgentCommand(
                DefaultSupportAgentUserName,
                DefaultSupportAgentEmail,
                DefaultSupportAgentPassword,
                invite.Code.CodeValue));
        });
        
        Assert.That(result, Is.Not.Null);
        
        
        var supportAgentId = new UserId(result.RefreshToken.UserId);
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(supportAgentId.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_organizationAdmin.OrganizationId!.IdValue);

        var persistedSupportAgent = await DbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == supportAgentId);
        
        Assert.That(persistedSupportAgent, Is.Not.Null);
        Assert.That(persistedSupportAgent.UserName.UserNameValue, Is.EqualTo(DefaultSupportAgentUserName));
        Assert.That(persistedSupportAgent.Email.EmailValue, Is.EqualTo(DefaultSupportAgentEmail));
        Assert.That(persistedSupportAgent.Role, Is.EqualTo(UserRole.SupportAgent));
        Assert.That(persistedSupportAgent.OrganizationId, Is.EqualTo(_organizationAdmin.OrganizationId));
        
        var persistedInvite = await DbContext.SupportAgentInvites
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invite.Id);
        
        Assert.That(persistedInvite, Is.Not.Null);
        Assert.That(persistedInvite.UsedAt, Is.Not.Null);
        Assert.That(persistedInvite.Status, Is.EqualTo(SupportAgentInviteStatus.Used));
        
        var persistedRefreshToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == result.RefreshToken.Value);
        
        Assert.That(persistedRefreshToken, Is.Not.Null);
        Assert.That(persistedRefreshToken.Token, Is.EqualTo(result.RefreshToken.Value));
        Assert.That(persistedRefreshToken.UserId, Is.EqualTo(supportAgentId.IdValue));       
    }

    [TestCase("", DefaultSupportAgentEmail, DefaultSupportAgentPassword)]
    [TestCase(DefaultSupportAgentUserName, "", DefaultSupportAgentPassword)]
    [TestCase(DefaultSupportAgentUserName, DefaultSupportAgentEmail, "")]
    [TestCase("", "", "")]  
    public async Task Handle_WithInvalidData_ShouldThrowValidationException(
        string userName,
        string email,
        string password)
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _organizationAdmin.OrganizationId!.IdValue, TimeProvider.System);
        
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new RegisterSupportAgentCommand(userName, email, password, invite.Code.CodeValue));
        });
    }

    [Test]
    public async Task Handle_WithOtherEmail_ShouldBreakSupportAgentInviteEmailMustMatchRule()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _organizationAdmin.OrganizationId!.IdValue, TimeProvider.System);

        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new RegisterSupportAgentCommand(
                DefaultSupportAgentUserName,
                DefaultCustomerEmail,
                DefaultSupportAgentPassword,
                invite.Code.CodeValue));
        });
        
        AssertUtility.AssertHasBrokenExactRule<SupportAgentInviteEmailMustMatchRule>(exception);
    }

    [Test]
    public async Task Handle_WithExpiredInvite_ShouldProduceExpiredInviteCodeError()
    {
        var fakeTimeProvider = new FakeTimeProvider(
            TimeProvider.System.GetUtcNow().UtcDateTime.AddDays(-(SupportAgentInviteOptionsDefaults.ExpirationDays + 1)));

        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _organizationAdmin.OrganizationId!.IdValue, fakeTimeProvider);
        
        fakeTimeProvider.Advance(TimeSpan.FromDays(SupportAgentInviteOptionsDefaults.ExpirationDays + 1));
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new RegisterSupportAgentCommand(
                DefaultSupportAgentUserName,
                DefaultSupportAgentEmail,
                DefaultSupportAgentPassword,
                invite.Code.CodeValue));
        });       
        
        AssertUtility.AssertHasProducedExactError(exception, SupportAgentInviteErrors.ExpiredInviteCode(invite.Code));
    }

    [Test]
    public async Task Handle_WithUsedInvite_ShouldProduceInvalidInviteCodeError()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _organizationAdmin.OrganizationId!.IdValue, TimeProvider.System);
        invite.Use(TimeProvider.System);
        await DbContext.SaveChangesAsync();
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new RegisterSupportAgentCommand(
                DefaultSupportAgentUserName,
                DefaultSupportAgentEmail,
                DefaultSupportAgentPassword,
                invite.Code.CodeValue));
        });       
        
        AssertUtility.AssertHasProducedExactError(exception, SupportAgentInviteErrors.InvalidInviteCode(invite.Code));       
    }
    
    [Test]
    public async Task Handle_WithRevokedInvite_ShouldProduceInvalidInviteCodeError()
    {
        var invite = await CreateSupportAgentInvite(DefaultSupportAgentEmail, _organizationAdmin.OrganizationId!.IdValue, TimeProvider.System);
        invite.Revoke(TimeProvider.System);
        await DbContext.SaveChangesAsync();
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new RegisterSupportAgentCommand(
                DefaultSupportAgentUserName,
                DefaultSupportAgentEmail,
                DefaultSupportAgentPassword,
                invite.Code.CodeValue));
        });       
        
        AssertUtility.AssertHasProducedExactError(exception, SupportAgentInviteErrors.InvalidInviteCode(invite.Code));       
    }
}