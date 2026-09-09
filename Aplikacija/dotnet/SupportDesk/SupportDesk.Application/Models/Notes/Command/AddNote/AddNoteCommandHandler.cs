using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Note.Repository;
using SupportDesk.Domain.Models.Note.Validation.Rules;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Repository;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.Repository;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Notes.Command.AddNote;

internal sealed class AddNoteCommandHandler
    : AbstractCommandHandler<AddNoteCommand, AddNoteCommandResult, AddNoteCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public AddNoteCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        IUserContext userContext,
        IUserRepository userRepository,
        ITicketRepository ticketRepository,
        INoteRepository noteRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _userContext = userContext;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _noteRepository = noteRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<AddNoteCommandHandlerContext> PrepareAsync(AddNoteCommand command, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(command.TicketId);
        
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        
        var userId = new UserId(_userContext.GetCurrentUserId());
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        return new AddNoteCommandHandlerContext(ticket, ticketId, user, userId);      
    }

    protected override async Task<AddNoteCommandResult> ExecuteAsync(AddNoteCommand command, AddNoteCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var userId = context.UserId;
        
        var ticket = context.Ticket!;

        var note = Note.Create(
            ticket.OrganizationId.IdValue,
            ticket.Id.IdValue,
            userId.IdValue,
            command.Text,
            _timeProvider);
        
        await _noteRepository.SaveAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new AddNoteCommandResult(note.Id.IdValue);     
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(AddNoteCommandHandlerContext context)
    {
        return 
        [
            [
                new DomainModelExistsRule<Ticket, TicketId>(context.Ticket, context.TicketId)
            ],
            [
                new UserCanAddNoteToTicketRule(context.Ticket!, context.User!)
            ]
        ];
    }
}