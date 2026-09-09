using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
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
        IServiceProvider serviceProvider,
        IUserContext userContext,
        ITenantContext tenantContext)
        : base(context, serviceProvider, userContext, tenantContext)
    {
        
    }
}