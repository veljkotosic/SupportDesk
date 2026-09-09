using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Repository;
using SupportDesk.Domain.Models.Ticket.Validation.Rules;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Command.CloseTicket;

internal sealed class CloseTicketCommandHandler
    : AbstractCommandHandler<CloseTicketCommand, CloseTicketCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CloseTicketCommandHandler(
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

    protected override async Task<CloseTicketCommandHandlerContext> PrepareAsync(CloseTicketCommand command, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(command.TicketId);
        
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        
        var userId = new UserId(_userContext.GetCurrentUserId());
        
        return new CloseTicketCommandHandlerContext(ticket, ticketId, userId);      
    }

    protected override async Task ExecuteAsync(CloseTicketCommand command, CloseTicketCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var ticket = context.Ticket!;
        
        ticket.Close(_timeProvider);
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);      
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(CloseTicketCommandHandlerContext context)
    {
        return
        [
            [
                new DomainModelExistsRule<Ticket, TicketId>(context.Ticket, context.TicketId)
            ],
            [
                new TicketMustBeClosedByTheSameUserWhoWasAssignedToItRule(context.Ticket!, context.UserId)
            ]
        ];
    }
}