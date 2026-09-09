using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.SupportAgentInvites.Command.RevokeSupportAgentInvite;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.Enums;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.IntegrationTests.Application.Models.SupportAgentInvites.Command;

[TestFixture]
internal sealed class RevokeSupportAgentInviteTests : IntegrationTestsBase
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
    public async Task Handle_WithPermissions_WithActiveInvite_ShouldRevokeInvite()
    {
        var invite = await CreateSupportAgentInvite(
            "valid@email.com",
            _organizationAdmin.OrganizationId!.IdValue,
            TimeProvider.System);
        DbContext.ChangeTracker.Clear();

        var command = new RevokeSupportAgentInviteCommand(invite.Id.IdValue);
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
        
        var persistedInvite = await DbContext.SupportAgentInvites
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invite.Id);
        
        Assert.That(persistedInvite, Is.Not.Null);
        Assert.That(persistedInvite.Status, Is.EqualTo(SupportAgentInviteStatus.Revoked));
        Assert.That(persistedInvite.RevokedAt, Is.Not.Null);
    }

    [Test]
    public async Task Handle_WithPermissions_WithUsedInvite_ShouldThrowValidationException()
    {
        var invite = await CreateSupportAgentInvite(
            "valid@email.com",
            _organizationAdmin.OrganizationId!.IdValue,
            TimeProvider.System);
        invite.Use(TimeProvider.System);
        
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new RevokeSupportAgentInviteCommand(invite.Id.IdValue);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithRevokedInvite_ShouldThrowValidationException()
    {
        var invite = await CreateSupportAgentInvite(
            "valid@email.com",
            _organizationAdmin.OrganizationId!.IdValue,
            TimeProvider.System);
        invite.Revoke(TimeProvider.System);
        
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new RevokeSupportAgentInviteCommand(invite.Id.IdValue);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
    }

    [Test]
    public async Task Handle_WithPermissions_WithInvalidInviteId_ShouldThrowValidationException()
    {
        var command = new RevokeSupportAgentInviteCommand(Guid.NewGuid());

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_organizationAdmin.Id.IdValue, Permissions.SupportAgentInvites.Revoke);
        
        var command = new RevokeSupportAgentInviteCommand(Guid.NewGuid());

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
            ;
        });
    }
}