using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.SupportAgentInvites.Command.CreateSupportAgentInvite;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.Enums;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.SupportAgentInvites.Command;

[TestFixture]
internal sealed class CreateSupportAgentInviteTests : IntegrationTestsBase
{
    private User _organizationAdmin;
    
    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await RegisterOrganizationAdmin();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_organizationAdmin.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_organizationAdmin.OrganizationId?.IdValue);
    }
    
    [Test]
    public async Task Handle_WithValidData_ShouldCreateSupportAgentInvite()
    {
        var email = "valid@email.com";
        var command = new CreateSupportAgentInviteCommand(email);

        CreateSupportAgentInviteCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(command);
        });
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.Not.Null);

        var code = new SupportAgentInviteCode(new Guid(result.Code));

        var persistedInvite = await DbContext.SupportAgentInvites
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Code == code);
        
        Assert.That(persistedInvite, Is.Not.Null);
        Assert.That(persistedInvite.OrganizationId, Is.EqualTo(_organizationAdmin.OrganizationId));
        Assert.That(persistedInvite.Email.EmailValue, Is.EqualTo(email));
        Assert.That(persistedInvite.Status, Is.EqualTo(SupportAgentInviteStatus.Active));
    }
    
    [Test]
    public async Task Handle_WithValidData_WithActiveInvite_ShouldCreateNewSupportAgentInvite_AndRevokeOldOne()
    {
        var email = "valid@email.com";
        
        var existingInvite = await CreateSupportAgentInvite(
            email,
            _organizationAdmin.OrganizationId!.IdValue,
            TimeProvider.System);
            
        var command = new CreateSupportAgentInviteCommand(email);

        CreateSupportAgentInviteCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(command);
        });
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.Not.Null);

        var code = new SupportAgentInviteCode(new Guid(result.Code));

        var persistedNewInvite = await DbContext.SupportAgentInvites
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Code == code);
        
        Assert.That(persistedNewInvite, Is.Not.Null);
        Assert.That(persistedNewInvite.OrganizationId, Is.EqualTo(_organizationAdmin.OrganizationId));
        Assert.That(persistedNewInvite.Email.EmailValue, Is.EqualTo(email));
        Assert.That(persistedNewInvite.Status, Is.EqualTo(SupportAgentInviteStatus.Active));
        
        var persistedOldInvite = await DbContext.SupportAgentInvites
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == existingInvite.Id);
        
        Assert.That(persistedOldInvite, Is.Not.Null);
        Assert.That(persistedOldInvite.Status, Is.EqualTo(SupportAgentInviteStatus.Revoked));       
        Assert.That(persistedOldInvite.RevokedAt, Is.Not.Null);
    }

    [Test]
    public async Task Handle_WithAlreadyRegisteredEmail_ShouldBreakSupportAgentInviteCannotBeCreatedForExistingUserRule()
    {
        var agent = await RegisterSupportAgent(_organizationAdmin.OrganizationId!.IdValue);
        
        var command = new CreateSupportAgentInviteCommand(agent.Email.EmailValue);
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
        
        AssertUtility.AssertHasBrokenExactRule<SupportAgentInviteCannotBeCreatedForExistingUserRule>(exception);
    }

    [Test]
    public async Task Handle_WithInvalidData_ShouldThrowValidationException()
    {
        var command = new CreateSupportAgentInviteCommand("invalidEmail");

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_organizationAdmin.Id.IdValue, Permissions.SupportAgentInvites.Create);
        
        var command = new CreateSupportAgentInviteCommand("valid@email.com");

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    }
}