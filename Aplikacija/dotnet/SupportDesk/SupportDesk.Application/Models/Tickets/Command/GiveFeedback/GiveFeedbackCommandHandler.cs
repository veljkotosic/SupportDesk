using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Repository;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Command.GiveFeedback;

internal sealed class GiveFeedbackCommandHandler
    : AbstractCommandHandler<GiveFeedbackCommand, GiveFeedbackCommandHandlerContext>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public GiveFeedbackCommandHandler(
        PermissionChecker permissionChecker,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<GiveFeedbackCommandHandlerContext> PrepareAsync(GiveFeedbackCommand command, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(command.TicketId);
        
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        
        return new GiveFeedbackCommandHandlerContext(ticket, ticketId);      
    }

    protected override async Task ExecuteAsync(GiveFeedbackCommand command, GiveFeedbackCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var ticket = context.Ticket!;
        
        ticket.GiveFeedback(command.Feedback);
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(GiveFeedbackCommandHandlerContext context)
    {
        return
        [
            [
                new DomainModelExistsRule<Ticket, TicketId>(context.Ticket, context.TicketId)
            ]
        ];
    }
}