using SupportDesk.Domain.Abstract;

namespace SupportDesk.Domain.Models.TemplateAnswer.Events;

public sealed record TemplateAnswerCreatedDomainEvent(Guid TemplateAnswerId) : IDomainEvent;