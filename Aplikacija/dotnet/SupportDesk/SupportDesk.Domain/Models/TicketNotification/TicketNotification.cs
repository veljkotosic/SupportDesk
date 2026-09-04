using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.TicketNotification.Events;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.TicketNotification;

public sealed class TicketNotification : AbstractDomainModel<TicketNotificationId>
{
    public OrganizationId OrganizationId { get; private set; }
    public TicketId TicketId { get; private set; }
    public TicketNotificationText Text { get; private set; }
    public TicketNotificationStatus Status { get; private set; }
    public CreatedAt CreatedAt { get; private set; }

    internal TicketNotification()
    {
        
    }
    
    private TicketNotification(
        TicketNotificationId id,
        OrganizationId organizationId,
        TicketId ticketId,
        TicketNotificationText text,
        TicketNotificationStatus status,
        CreatedAt createdAt
        ) : base(id)
    {
        OrganizationId = organizationId;
        TicketId = ticketId;
        Text = text;
        Status = status;
        CreatedAt = createdAt;   
    }

    public static TicketNotification Create(Guid organizationId, Guid ticketId, string text, Guid customerId, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;    
        
        var idVo = TicketNotificationId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var ticketIdVo = new TicketId(ticketId);
        var textVo = new TicketNotificationText(text);
        var status = TicketNotificationStatus.Unread;
        var createdAtVo = new CreatedAt(now);

        var createdTicketNotification = new TicketNotification(
            idVo,
            organizationIdVo,
            ticketIdVo,
            textVo,
            status,
            createdAtVo);
        
        var customerIdVo = new UserId(customerId);
        
        createdTicketNotification.RaiseDomainEvent(new TicketNotificationCreatedDomainEvent(createdTicketNotification.Id, customerIdVo));
        
        return createdTicketNotification;       
    }
}