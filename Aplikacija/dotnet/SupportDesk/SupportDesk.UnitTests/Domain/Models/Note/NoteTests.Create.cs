using SupportDesk.Domain.Abstract.Validation;
using NoteModel = SupportDesk.Domain.Models.Note.Note;

namespace SupportDesk.UnitTests.Domain.Models.Note;

[TestFixture]
internal sealed class NoteTests
{
    [TestCase("Test")]
    public void Create_WithValidData_ShouldCreateNote(string validText)
    {
        var timeProvider = TimeProvider.System;
        
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var note = NoteModel.Create(organizationId, ticketId, authorId, validText, timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(note.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(note.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(note.TicketId.IdValue, Is.EqualTo(ticketId));
            Assert.That(note.AuthorId.IdValue, Is.EqualTo(authorId));
            Assert.That(note.Text.TextValue, Is.EqualTo(validText));
            Assert.That(note.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
        });
    }

    [TestCase("")]
    public void Create_WithInvalidData_ShouldThrowValidationException(string invalidText)
    {
        var timeProvider = TimeProvider.System;
        
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        Assert.Throws<ValidationException>(() =>
        {
            _ = NoteModel.Create(organizationId, ticketId, authorId, invalidText, timeProvider);
        });
    }
}