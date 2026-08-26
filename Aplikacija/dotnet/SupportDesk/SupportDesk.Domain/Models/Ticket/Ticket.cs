using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Events;
using SupportDesk.Domain.Models.Ticket.Validation.Rules;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket;

public sealed class Ticket : AbstractDomainModel<TicketId>
{
    public OrganizationId OrganizationId { get; private set; }
    public UserId CustomerId { get; private set; }
    public UserId? SupportAgentId { get; private set; }
    public CategoryId CategoryId { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketFeedback Feedback { get; private set; }
    public TicketSubject Subject { get; private set; }
    public TicketOpenedAt OpenedAt { get; private set; }
    public TicketAssignedAt? AssignedAt { get; private set; }
    public TicketClosedAt? ClosedAt { get; private set; }
    public TicketLastMessageAt? LastMessageAt { get; private set; }

    private Ticket(
        TicketId id,
        OrganizationId organizationId,
        UserId customerId,
        UserId? supportAgentId,
        CategoryId categoryId,
        TicketStatus status,
        TicketPriority priority,
        TicketFeedback feedback,
        TicketSubject subject,
        TicketOpenedAt openedAt,
        TicketAssignedAt? assignedAt,
        TicketClosedAt? closedAt,
        TicketLastMessageAt? lastMessageAt
        ) : base(id)
    {
        OrganizationId = organizationId;
        CustomerId = customerId;
        SupportAgentId = supportAgentId;
        CategoryId = categoryId;
        Status = status;
        Priority = priority;
        Feedback = feedback;
        Subject = subject;
        OpenedAt = openedAt;
        AssignedAt = assignedAt;
        ClosedAt = closedAt;
        LastMessageAt = lastMessageAt;
        
        ValidateModel();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new TicketCannotBeAssignedBeforeItsOpenedRule(OpenedAt, AssignedAt),
            new TicketCannotBeClosedBeforeItsAssignedRule(AssignedAt, ClosedAt),
            new TicketLastMessageCannotOccurBeforeItsOpenedRule(OpenedAt, LastMessageAt),
            new TicketLastMessageCannotOccurAfterItsClosedRule(ClosedAt, LastMessageAt)
        ];
    }

    public static Ticket Create(
        Guid organizationId,
        Guid customerId,
        Guid categoryId,
        TicketPriority priority,
        string subject
    )
    {
        var idVo = TicketId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var customerIdVo = new UserId(customerId);
        UserId? supportAgentVo = null;
        var categoryIdVo = new CategoryId(categoryId);
        var status = TicketStatus.Open;
        var feedback = TicketFeedback.None;
        var subjectVo = new TicketSubject(subject);
        var openedAtVo = new TicketOpenedAt(DateTime.UtcNow);
        TicketAssignedAt? assignedAtVo = null;
        TicketClosedAt? closedAtVo = null;
        TicketLastMessageAt? lastMessageAtVo = null;

        var createdTicket = new Ticket(
            idVo,
            organizationIdVo,
            customerIdVo,
            supportAgentVo,
            categoryIdVo,
            status,
            priority,
            feedback,
            subjectVo,
            openedAtVo,
            assignedAtVo,
            closedAtVo,
            lastMessageAtVo);
        
        createdTicket.RaiseDomainEvent(new TicketCreatedDomainEvent(createdTicket.Id.IdValue));
        
        return createdTicket;       
    }
}