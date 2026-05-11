using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Note.Repository;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Note.AddNote;

public class AddNoteRequestHandler
    : IRequestHandler<AddNoteRequest, AddNoteResult>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddNoteRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        INoteRepository noteRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _noteRepository = noteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddNoteResult> HandleAsync(AddNoteRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        var organizationId = _userContext.GetCurrentUsersOrganizationId()!;
        
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        
        if (ticket is null)
        {
            throw new Exception("Ticket not found.");
        }

        var note = new Data.Entities.Note.Note
        {
            Id = Guid.NewGuid(),
            AuthorId = userId,
            OrganizationId = (Guid)organizationId,
            TicketId = request.TicketId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };
        
        await _noteRepository.SaveAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new AddNoteResult(note.Id);       
    }
}