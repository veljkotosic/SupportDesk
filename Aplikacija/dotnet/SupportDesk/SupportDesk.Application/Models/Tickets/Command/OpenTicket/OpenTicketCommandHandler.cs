using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.Repository;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Message.Repository;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.Repository;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Repository;

namespace SupportDesk.Application.Models.Tickets.Command.OpenTicket;

internal sealed class OpenTicketCommandHandler
    : AbstractCommandHandler<OpenTicketCommand, OpenTicketCommandResult, OpenTicketCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public OpenTicketCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        IUserContext userContext,
        IOrganizationRepository organizationRepository,
        ICategoryRepository categoryRepository,
        ITicketRepository ticketRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _userContext = userContext;
        _organizationRepository = organizationRepository;
        _categoryRepository = categoryRepository;
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<OpenTicketCommandHandlerContext> PrepareAsync(OpenTicketCommand command, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(command.OrganizationId);
        var categoryId = new CategoryId(command.CategoryId);
        
        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);
        var category = await _categoryRepository.GetByIdAndOrganizationIdAsync(categoryId, organizationId, cancellationToken);

        return new OpenTicketCommandHandlerContext(organization, organizationId, category, categoryId);
    }

    protected override async Task<OpenTicketCommandResult> ExecuteAsync(OpenTicketCommand command, OpenTicketCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var organization = context.Organization!;
        var category = context.Category!;
        
        var ticket = Ticket.Open(
            organization.Id.IdValue,
            _userContext.GetCurrentUserId(),
            category.Id.IdValue,
            command.Priority,
            command.Subject,
            _timeProvider);
        
        var initialMessage = Message.Create(
            organization.Id.IdValue,
            ticket.Id.IdValue,
            _userContext.GetCurrentUserId(),
            command.InitialMessage,
            _timeProvider);
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _messageRepository.SaveAsync(initialMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new OpenTicketCommandResult(ticket.Id.IdValue);     
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(OpenTicketCommandHandlerContext context)
    {
        return
        [
            [
                new DomainModelExistsRule<Organization, OrganizationId>(context.Organization, context.OrganizationId),
                new DomainModelExistsRule<Category, CategoryId>(context.Category, context.CategoryId)
            ]
        ];
    }
}