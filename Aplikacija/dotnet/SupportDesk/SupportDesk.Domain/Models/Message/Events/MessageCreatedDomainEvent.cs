using SupportDesk.Domain.Abstract;

namespace SupportDesk.Domain.Models.Message.Events;

public sealed record MessageCreatedDomainEvent(Guid MessageId) : IDomainEvent;