using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Message.ValueObjects;

namespace SupportDesk.Domain.Models.Message.Events;

public sealed record MessageCreatedDomainEvent(MessageId MessageId) : IDomainEvent;