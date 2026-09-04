using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Messages.Command.SendMessage;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Message.ValueObjects;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.IntegrationTests.Application.Models.Messages.Command;

[TestFixture]
internal sealed class SendMessageTests : IntegrationTestsBase
{
    private const string ValidMessage = "Test Message";
    
    private const string InvalidMessage = "";
    
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
    }
    
    [Test]
    public async Task Handle_WithPermissions_AsCustomer_WithValidData_ShouldSendMessage()
    {
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_customer.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_customer.OrganizationId?.IdValue);

        var command = new SendMessageCommand(_ticket.Id.IdValue, ValidMessage);
        
        SendMessageCommandResult? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(command);
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.MessageId, Is.Not.EqualTo(Guid.Empty));
        
        var messageId = new MessageId(result.MessageId);
        
        var persistedMessage = await DbContext.Messages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);
        
        Assert.That(persistedMessage, Is.Not.Null);
        Assert.That(persistedMessage.Text.TextValue, Is.EqualTo(ValidMessage));
        Assert.That(persistedMessage.OrganizationId, Is.EqualTo(_organizationAdmin.OrganizationId));
        Assert.That(persistedMessage.SenderId, Is.EqualTo(_customer.Id));
        Assert.That(persistedMessage.TicketId, Is.EqualTo(_ticket.Id));
    }

    [Test]
    public async Task Handle_WithPermissions_AsCustomer_WithInvalidData_ShouldThrowValidationException()
    {
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_customer.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_customer.OrganizationId?.IdValue);

        var command = new SendMessageCommand(_ticket.Id.IdValue, InvalidMessage);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    } 
    
    [Test]
    public async Task Handle_WithPermissions_AsCustomer_WithInvalidTicketId_ShouldThrowValidationException()
    {
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_customer.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_customer.OrganizationId?.IdValue);

        var command = new SendMessageCommand(Guid.NewGuid(), ValidMessage);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_customer.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_customer.OrganizationId?.IdValue);

        await PermissionService.RevokePermissionAsync(_customer.Id.IdValue, Permissions.Messages.Send);
        
        var command = new SendMessageCommand(Guid.NewGuid(), ValidMessage);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    }

    [Test]
    public async Task Handle_AsSupportAgent_WithTicketOpen_ShouldThrowValidationException()
    {
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_supportAgent.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_supportAgent.OrganizationId?.IdValue);
        
        var command = new SendMessageCommand(_ticket.Id.IdValue, ValidMessage);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);       
        });
    }
    
    [Test]
    public async Task Handle_AsSupportAgent_WithTicketAssigned_ShouldSendMessage()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_supportAgent.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_supportAgent.OrganizationId?.IdValue);
        
        var command = new SendMessageCommand(_ticket.Id.IdValue, ValidMessage);

        SendMessageCommandResult? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(command);
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.MessageId, Is.Not.EqualTo(Guid.Empty));
        
        var messageId = new MessageId(result.MessageId);
        
        var persistedMessage = await DbContext.Messages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);
        
        Assert.That(persistedMessage, Is.Not.Null);
        Assert.That(persistedMessage.Text.TextValue, Is.EqualTo(ValidMessage));
        Assert.That(persistedMessage.OrganizationId, Is.EqualTo(_organizationAdmin.OrganizationId));
        Assert.That(persistedMessage.SenderId, Is.EqualTo(_supportAgent.Id));
        Assert.That(persistedMessage.TicketId, Is.EqualTo(_ticket.Id));
    }
    
    [Test]
    public async Task Handle_WithTicketClosed_ShouldThrowValidationException()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        _ticket.Close(TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_supportAgent.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_supportAgent.OrganizationId?.IdValue);
        
        var command = new SendMessageCommand(_ticket.Id.IdValue, ValidMessage);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);       
        });
    }
}