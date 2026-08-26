using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.Note.ValueObjects;

namespace SupportDesk.Domain.Models.Note.Repository;

public interface INoteRepository : IAbstractRepository<Note, NoteId>
{
    
}