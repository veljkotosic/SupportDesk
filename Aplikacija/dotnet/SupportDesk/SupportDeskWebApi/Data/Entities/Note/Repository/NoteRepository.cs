using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.Note.Repository;

public class NoteRepository : Repository<Note>, INoteRepository
{
    public NoteRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }
}