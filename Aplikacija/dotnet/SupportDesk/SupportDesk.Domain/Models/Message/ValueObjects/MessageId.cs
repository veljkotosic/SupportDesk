using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.Message.ValueObjects;

public sealed record MessageId(Guid IdValue) : DomainId(IdValue)
{
    public static MessageId NewId() => new MessageId(Guid.NewGuid());
}