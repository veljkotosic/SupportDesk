using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Faq.ValueObjects;

namespace SupportDesk.Domain.Models.Faq.Events;

public sealed record FaqCreatedDomainEvent(FaqId FaqId) : IDomainEvent;