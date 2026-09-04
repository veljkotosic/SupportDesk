using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Tickets.Command.OpenTicket;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.IntegrationTests.Application.Models.Tickets.Command;

[TestFixture]
internal sealed class OpenTicketTests : IntegrationTestsBase
{
    private const TicketPriority ValidPriority = TicketPriority.High;
    private const string ValidSubject = "Test Subject";
    private const string ValidMessage = "Test Message";
    
    private const string InvalidSubject = "";
    private const string InvalidMessage = "";
    
    [Test]
    public async Task Handle_WithPermissions_WithValidData_ShouldOpenTicket()
    {
        var organizationAdmin = await RegisterOrganizationAdmin();
        var category = await CreateCategory(organizationAdmin.OrganizationId!.IdValue, "Test", "Test", TimeProvider.System);
        
        var customer = await RegisterCustomer();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(customer.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(customer.OrganizationId?.IdValue);

        var command = new OpenTicketCommand(
            organizationAdmin.OrganizationId.IdValue,
            category.Id.IdValue,
            ValidPriority,
            ValidSubject,
            ValidMessage);
        
        OpenTicketCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(command);
        });
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TicketId, Is.Not.EqualTo(Guid.Empty));

        var ticketId = new TicketId(result.TicketId);
        
        var persistedTicket = await DbContext.Tickets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        
        Assert.That(persistedTicket, Is.Not.Null);
        Assert.That(persistedTicket.Status, Is.EqualTo(TicketStatus.Open));
        Assert.That(persistedTicket.OrganizationId, Is.EqualTo(organizationAdmin.OrganizationId));       
        Assert.That(persistedTicket.CustomerId, Is.EqualTo(customer.Id));      
        Assert.That(persistedTicket.CategoryId, Is.EqualTo(category.Id));
        Assert.That(persistedTicket.Priority, Is.EqualTo(ValidPriority));      
        Assert.That(persistedTicket.Subject.SubjectValue, Is.EqualTo(ValidSubject));      
        
        var persistedMessage = await DbContext.Messages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.TicketId == ticketId);
        
        Assert.That(persistedMessage, Is.Not.Null);
        Assert.That(persistedMessage.OrganizationId, Is.EqualTo(organizationAdmin.OrganizationId));
        Assert.That(persistedMessage.SenderId, Is.EqualTo(customer.Id));
        Assert.That(persistedMessage.Text.TextValue, Is.EqualTo(ValidMessage));
    }

    [TestCase(InvalidSubject, ValidMessage)]
    [TestCase(ValidSubject, InvalidMessage)]
    [TestCase(InvalidSubject, InvalidMessage)] 
    public async Task Handle_WithPermissions_WithInvalidData_ShouldThrowValidationException(string subject, string message)
    {
        var organizationAdmin = await RegisterOrganizationAdmin();
        var category = await CreateCategory(organizationAdmin.OrganizationId!.IdValue, "Test", "Test", TimeProvider.System);
        
        var customer = await RegisterCustomer();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(customer.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(customer.OrganizationId?.IdValue);

        var command = new OpenTicketCommand(
            organizationAdmin.OrganizationId.IdValue,
            category.Id.IdValue,
            TicketPriority.Medium,
            subject,
            message);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    }

    [Test]
    public async Task Handle_WithoutPermissions_shouldThrowPermissionException()
    {
        var customer = await RegisterCustomer();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(customer.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(customer.OrganizationId?.IdValue);

        await PermissionService.RevokePermissionAsync(customer.Id.IdValue, Permissions.Tickets.Open);

        var command = new OpenTicketCommand(Guid.NewGuid(), Guid.NewGuid(), TicketPriority.Medium, "Test", "Test");
        
        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    }
}