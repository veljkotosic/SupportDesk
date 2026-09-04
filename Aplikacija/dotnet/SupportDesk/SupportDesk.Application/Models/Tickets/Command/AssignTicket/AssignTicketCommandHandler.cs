using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Repository;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Command.AssignTicket;

internal sealed class AssignTicketCommandHandler
    : AbstractCommandHandler<AssignTicketCommand, AssignTicketCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public AssignTicketCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        IUserContext userContext,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<AssignTicketCommandHandlerContext> PrepareAsync(AssignTicketCommand command, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(command.TicketId);
        
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        
        return new AssignTicketCommandHandlerContext(ticket, ticketId);       
    }

    protected override async Task ExecuteAsync(AssignTicketCommand command, AssignTicketCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var ticket = context.Ticket!;
        
        ticket.Assign(userId, _timeProvider);
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);       
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(AssignTicketCommandHandlerContext context)
    {
        return
        [
            [
                new DomainModelExistsRule<Ticket, TicketId>(context.Ticket, context.TicketId)
            ]
        ];
    }
}