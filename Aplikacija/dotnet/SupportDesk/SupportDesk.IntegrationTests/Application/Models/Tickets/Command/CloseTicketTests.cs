using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Tickets.Command.CloseTicket;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Validation.Rules;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Tickets.Command;

[TestFixture]
internal sealed class CloseTicketTests : IntegrationTestsBase
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
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_supportAgent.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_supportAgent.OrganizationId?.IdValue);
    }
    
    [Test]
    public async Task Handle_WithPermissions_ShouldCloseTicket()
    {
        _ticket.Assign(_supportAgent.Id.IdValue,  TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var command = new CloseTicketCommand(_ticket.Id.IdValue);
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
        
        var persistedTicket = await DbContext.Tickets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == _ticket.Id);
        
        Assert.That(persistedTicket, Is.Not.Null);
        Assert.That(persistedTicket.Status, Is.EqualTo(TicketStatus.Closed));
        Assert.That(persistedTicket.ClosedAt, Is.Not.Null);
        Assert.That(persistedTicket.ClosedAt.ClosedAtValue, Is.Not.Default);
    }
    
    [Test]
    public void Handle_WithPermissions_WithTicketOpen_ShouldThrowValidationException()
    {
        var command = new CloseTicketCommand(_ticket.Id.IdValue);
        
        Assert.ThrowsAsync<ValidationException>(async () => await CommandDispatcher.DispatchAsync(command));
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithTicketAlreadyClosed_ShouldThrowValidationException()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var command = new CloseTicketCommand(_ticket.Id.IdValue);
        
        Assert.ThrowsAsync<ValidationException>(async () => await CommandDispatcher.DispatchAsync(command));
    }

    [Test]
    public async Task Handle_WithOtherSupportAgentBeingAssigned_ShouldBreakTicketMustBeClosedByTheSameUserWhoWasAssignedToItRule()
    {
        var otherSupportAgent = await RegisterSupportAgent(
            _organizationAdmin.OrganizationId!.IdValue,
            "otheragent@test.com",
            "Other Agent");
        
        _ticket.Assign(otherSupportAgent.Id.IdValue, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var command = new CloseTicketCommand(_ticket.Id.IdValue);

        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
        
        AssertUtility.AssertHasBrokenExactRule<TicketMustBeClosedByTheSameUserWhoWasAssignedToItRule>(exception);
    }
    
    [Test]
    public async Task Handle_WithNoPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_supportAgent.Id.IdValue, Permissions.Tickets.Close);
        
        var command = new CloseTicketCommand(_ticket.Id.IdValue);

        Assert.ThrowsAsync<PermissionException>(async () => await CommandDispatcher.DispatchAsync(command));
    }
}