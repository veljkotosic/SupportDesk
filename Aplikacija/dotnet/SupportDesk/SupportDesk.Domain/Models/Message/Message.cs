using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Message.Events;
using SupportDesk.Domain.Models.Message.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.Message;

public sealed class Message : AbstractDomainModel<MessageId>
{
    public OrganizationId OrganizationId { get; private set; }
    public TicketId TicketId { get; private set; }
    public UserId SenderId { get; private set; }
    public MessageText Text { get; private set; }
    public CreatedAt CreatedAt { get; private set; }

    private Message(
        MessageId id,
        OrganizationId organizationId,
        TicketId ticketId,
        UserId senderId,
        MessageText text,
        CreatedAt createdAt
        ) : base(id)
    {
        OrganizationId = organizationId;
        TicketId = ticketId;
        SenderId = senderId;
        Text = text;
        CreatedAt = createdAt;
    }

    public static Message Create(Guid organizationId, Guid ticketId, Guid senderId, string text)
    {
        var idVo = MessageId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var ticketIdVo = new TicketId(ticketId);
        var senderIdVo = new UserId(senderId);
        var textVo = new MessageText(text);
        var createdAtVo = new CreatedAt(DateTime.UtcNow);

        var createdMessage = new Message(
            idVo,
            organizationIdVo,
            ticketIdVo,
            senderIdVo,
            textVo,
            createdAtVo);
        
        createdMessage.RaiseDomainEvent(new MessageCreatedDomainEvent(createdMessage.Id.IdValue));
        
        return createdMessage;      
    }
}