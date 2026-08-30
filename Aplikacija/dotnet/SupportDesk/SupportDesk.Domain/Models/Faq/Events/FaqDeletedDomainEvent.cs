using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Faq.ValueObjects;

namespace SupportDesk.Domain.Models.Faq.Events;

public sealed record FaqDeletedDomainEvent(FaqId FaqId) : IDomainEvent;