using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Tickets.Command.ReadAllNotifications;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Validation.Rules;
using SupportDesk.Domain.Models.TicketNotification;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Tickets.Command;

[TestFixture]
internal sealed class ReadAllNotificationsTests : IntegrationTestsBase
{
    private User _organizationAdmin = null!;
    private User _supportAgent = null!;
    private User _customer = null!;
    private Category _category = null!;
    private Ticket _ticket = null!;

    [SetUp]
    public async Task Setup()
    {
        _organizationAdmin = await RegisterOrganizationAdmin();
        _supportAgent = await RegisterSupportAgent(_organizationAdmin.OrganizationId!.IdValue);
        _category = await CreateCategory(_organizationAdmin.OrganizationId!.IdValue, "Test", "Test", TimeProvider.System);
        
        _customer = await RegisterCustomer();
        
        _ticket = await CreateTicket(
            _organizationAdmin.OrganizationId.IdValue, 
            _category.Id.IdValue,
            _customer.Id.IdValue,
            TicketPriority.Medium,
            "Test Ticket",
            "Test Message",
            TimeProvider.System);
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_customer.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_customer.OrganizationId?.IdValue);
    }

    [Test]
    public async Task Handle_WithPermissions_WithValidTicketId_ShouldReadAllNotifications()
    {
        await CreateTicketNotification(
            _organizationAdmin.OrganizationId!.IdValue,
            _ticket.Id.IdValue,
            _customer.Id.IdValue,
            "Notification 1");

        await CreateTicketNotification(
            _organizationAdmin.OrganizationId!.IdValue,
            _ticket.Id.IdValue,
            _customer.Id.IdValue,
            "Notification 2");

        var command = new ReadAllNotificationsCommand(_ticket.Id.IdValue);
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
        
        var persistedNotifications = await DbContext.Set<TicketNotification>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(n => n.TicketId == _ticket.Id)
            .ToListAsync();

        Assert.That(persistedNotifications, Has.Count.EqualTo(2));
        Assert.That(persistedNotifications.All(n => n.Status == TicketNotificationStatus.Read), Is.True);
    }

    [Test]
    public async Task Handle_WithPermissions_WithUnexistingTicketId_ShouldThrowValidationException()
    {
        var command = new ReadAllNotificationsCommand(Guid.NewGuid());
        
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
    }

    [Test]
    public async Task Handle_WithPermissions_WithOtherUsersTicket_ShouldBreakTicketBelongsToUserRule()
    {
        var otherUser = await RegisterCustomer("other@email.com", "Other User");
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(otherUser.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(otherUser.OrganizationId?.IdValue);       
        
        var command = new ReadAllNotificationsCommand(_ticket.Id.IdValue);
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
        
        AssertUtility.AssertHasBrokenExactRule<TicketBelongsToUserRule>(exception);
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_customer.Id.IdValue, Permissions.Tickets.ReadNotifications);
        
        var command = new ReadAllNotificationsCommand(_ticket.Id.IdValue);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
    }
}