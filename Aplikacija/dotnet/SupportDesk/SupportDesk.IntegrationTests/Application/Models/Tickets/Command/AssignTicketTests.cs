using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Tickets.Command.AssignTicket;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.IntegrationTests.Application.Models.Tickets.Command;

[TestFixture]
internal sealed class AssignTicketTests : IntegrationTestsBase
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
    public async Task Handle_WithPermissions_ShouldAssignTicket()
    {
        var command = new AssignTicketCommand(_ticket.Id.IdValue);
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
        
        var persistedTicket = await DbContext.Tickets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == _ticket.Id);
        
        Assert.That(persistedTicket, Is.Not.Null);
        Assert.That(persistedTicket.Status, Is.EqualTo(TicketStatus.Assigned));
        Assert.That(persistedTicket.AssignedAt, Is.Not.Null);
        Assert.That(persistedTicket.AssignedAt.AssignedAtValue, Is.Not.Default);
        Assert.That(persistedTicket.SupportAgentId, Is.EqualTo(_supportAgent.Id));
    }

    [Test]
    public async Task Handle_WithPermissions_WithTicketAlreadyAssigned_ShouldThrowValidationException()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var command = new AssignTicketCommand(_ticket.Id.IdValue);
        
        Assert.ThrowsAsync<ValidationException>(async () => await CommandDispatcher.DispatchAsync(command));
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithTicketAlreadyClosed_ShouldThrowValidationException()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var command = new AssignTicketCommand(_ticket.Id.IdValue);
        
        Assert.ThrowsAsync<ValidationException>(async () => await CommandDispatcher.DispatchAsync(command));
    }
    
    [Test]
    public async Task Handle_WithNoPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_supportAgent.Id.IdValue, Permissions.Tickets.Assign);
        
        var command = new AssignTicketCommand(_ticket.Id.IdValue);

        Assert.ThrowsAsync<PermissionException>(async () => await CommandDispatcher.DispatchAsync(command));
    }
}