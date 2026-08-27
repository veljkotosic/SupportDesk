using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Note.Events;
using SupportDesk.Domain.Models.Note.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.Note;

public sealed class Note : AbstractDomainModel<NoteId>
{
    public OrganizationId OrganizationId { get; private set; }
    public TicketId TicketId { get; private set; }
    public UserId AuthorId { get; private set; }
    public NoteText Text { get; private set; }
    public CreatedAt CreatedAt { get; private set; }

    internal Note()
    {
        
    }
    
    private Note(
        NoteId id,
        OrganizationId organizationId,
        TicketId ticketId,
        UserId authorId,
        NoteText text,
        CreatedAt createdAt
    ) : base(id)
    {
        OrganizationId = organizationId;
        TicketId = ticketId;
        AuthorId = authorId;
        Text = text;
        CreatedAt = createdAt;
    }

    public static Note Create(Guid organizationId, Guid ticketId, Guid authorId, string text, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;    
        
        var idVo = NoteId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var ticketIdVo = new TicketId(ticketId);
        var authorIdVo = new UserId(authorId);
        var textVo = new NoteText(text);
        var createdAtVo = new CreatedAt(now);

        var createdNote = new Note(idVo, organizationIdVo, ticketIdVo, authorIdVo, textVo, createdAtVo);
        
        createdNote.RaiseDomainEvent(new NoteCreatedDomainEvent(createdNote.Id.IdValue));
        
        return createdNote;
    }
}