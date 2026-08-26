using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Note.Repository;
using SupportDesk.Domain.Models.Note.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Notes;

public sealed class NoteRepository
    : AbstractRepository<Note, NoteId>, INoteRepository
{
    public NoteRepository(
        SupportDeskDbContext context,
        IDomainEventCollector domainEventCollector) 
        : base(context, domainEventCollector)
    {
        
    }
}