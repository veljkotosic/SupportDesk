using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Tickets.Command.GiveFeedback;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.IntegrationTests.Application.Models.Tickets.Command;

[TestFixture]
internal sealed class GiveFeedbackTests : IntegrationTestsBase
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
    public async Task Handle_WithPermissions_WithValidData_ShouldGiveFeedback()
    {
        _ticket.Assign(_supportAgent.Id.IdValue,  TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        var command = new GiveFeedbackCommand(_ticket.Id.IdValue, TicketFeedback.Helpful);

        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });

        var persistedTicket = await DbContext.Tickets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == _ticket.Id);
        
        Assert.That(persistedTicket, Is.Not.Null);
        Assert.That(persistedTicket.Feedback, Is.EqualTo(TicketFeedback.Helpful));
    }

    [Test]
    public async Task Handle_WithPermissions_WithUnexistingTicket_ShouldThrowValidationException()
    {
        var command = new GiveFeedbackCommand(Guid.NewGuid(), TicketFeedback.Helpful);
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        }); 
    }

    [Test]
    public async Task Handle_WithPermissions_WithTicketAlreadyGivenFeedback_ShouldThrowValidationException()
    {
        _ticket.Assign(_supportAgent.Id.IdValue,  TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        _ticket.GiveFeedback(TicketFeedback.Helpful);
        await UnitOfWork.SaveChangesAsync();
        
        var command = new GiveFeedbackCommand(_ticket.Id.IdValue, TicketFeedback.Helpful);
        
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });       
    }

    [Test]
    public async Task Handle_WithPermissions_WithInvalidData_ShouldThrowValidationException()
    {
        _ticket.Assign(_supportAgent.Id.IdValue,  TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        _ticket.GiveFeedback(TicketFeedback.Helpful);
        await UnitOfWork.SaveChangesAsync();
        
        var command = new GiveFeedbackCommand(_ticket.Id.IdValue, TicketFeedback.None);
        
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        }); 
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithTicketNotClosed_ShouldThrowValidationException()
    {
        var command = new GiveFeedbackCommand(_ticket.Id.IdValue, TicketFeedback.Helpful);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
    }
    
    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_customer.Id.IdValue, Permissions.Tickets.GiveFeedback);
        
        var command = new GiveFeedbackCommand(_ticket.Id.IdValue, TicketFeedback.Helpful);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(command);
        });
    }
}