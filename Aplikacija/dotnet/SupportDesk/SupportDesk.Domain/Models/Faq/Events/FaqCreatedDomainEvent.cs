using SupportDesk.Domain.Abstract;

namespace SupportDesk.Domain.Models.Faq.Events;

public sealed record FaqCreatedDomainEvent(Guid FaqId) : IDomainEvent;