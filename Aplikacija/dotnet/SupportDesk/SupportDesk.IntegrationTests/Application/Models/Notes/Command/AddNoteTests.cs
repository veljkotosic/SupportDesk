using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Notes.Command.AddNote;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Note.Validation.Rules;
using SupportDesk.Domain.Models.Note.ValueObjects;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Notes.Command;

[TestFixture]
internal sealed class AddNoteTests : IntegrationTestsBase
{
    private const string ValidText = "Test Message";
    
    private const string InvalidText = "";
    
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
    public async Task Handle_AsSupportAgent_WithPermissions_WithValidData_ShouldAddNote()
    {
        _ticket.Assign(_supportAgent.Id.IdValue, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_supportAgent.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_supportAgent.OrganizationId?.IdValue);
        DbContext.ChangeTracker.Clear();
        
        var command = new AddNoteCommand(_ticket.Id.IdValue, ValidText);
        
        AddNoteCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(command);
        });
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NoteId, Is.Not.EqualTo(Guid.Empty));
        
        var noteId = new NoteId(result.NoteId);

        var persistedNote = await DbContext.Notes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == noteId);
        
        Assert.That(persistedNote, Is.Not.Null);
        Assert.That(persistedNote.Text.TextValue, Is.EqualTo(ValidText));
        Assert.That(persistedNote.OrganizationId, Is.EqualTo(_supportAgent.OrganizationId));
        Assert.That(persistedNote.TicketId, Is.EqualTo(_ticket.Id));
        Assert.That(persistedNote.AuthorId, Is.EqualTo(_supportAgent.Id));
    }
    
    [Test]
    public async Task Handle_AsOrganizationAdmin_WithPermissions_WithValidData_ShouldAddNote()
    {
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_organizationAdmin.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_organizationAdmin.OrganizationId?.IdValue);
        DbContext.ChangeTracker.Clear();
        
        var command = new AddNoteCommand(_ticket.Id.IdValue, ValidText);
        
        AddNoteCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(command);
        });
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NoteId, Is.Not.EqualTo(Guid.Empty));
        
        var noteId = new NoteId(result.NoteId);

        var persistedNote = await DbContext.Notes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == noteId);
        
        Assert.That(persistedNote, Is.Not.Null);
        Assert.That(persistedNote.Text.TextValue, Is.EqualTo(ValidText));
        Assert.That(persistedNote.OrganizationId, Is.EqualTo(_organizationAdmin.OrganizationId));
        Assert.That(persistedNote.TicketId, Is.EqualTo(_ticket.Id));
        Assert.That(persistedNote.AuthorId, Is.EqualTo(_organizationAdmin.Id));
    }

    [Test]
    public async Task Handle_WithInvalidData_ShouldThrowValidationException()
    {
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_organizationAdmin.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_organizationAdmin.OrganizationId?.IdValue);
        DbContext.ChangeTracker.Clear();
        
        var command = new AddNoteCommand(Guid.NewGuid(), InvalidText);

        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    }

    [Test]
    public async Task Handle_WithOtherSupportAgentAssigned_ShouldBreakUserCanAddNoteToTicketRule()
    {
        var otherAgent = await RegisterSupportAgent(
            _organizationAdmin.OrganizationId!.IdValue,
            "otheragent@gmail.com",
            "Other Agent");
        
        _ticket.Assign(otherAgent.Id.IdValue, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_supportAgent.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_supportAgent.OrganizationId?.IdValue);
        DbContext.ChangeTracker.Clear();
        
        var command = new AddNoteCommand(_ticket.Id.IdValue, ValidText);
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
        
        AssertUtility.AssertHasBrokenExactRule<UserCanAddNoteToTicketRule>(exception);
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_supportAgent.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_supportAgent.OrganizationId?.IdValue);
        DbContext.ChangeTracker.Clear();
        
        await PermissionService.RevokePermissionAsync(_supportAgent.Id.IdValue, Permissions.Notes.Add);
        
        var command = new AddNoteCommand(_ticket.Id.IdValue, ValidText);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(command);
        });
    }
}