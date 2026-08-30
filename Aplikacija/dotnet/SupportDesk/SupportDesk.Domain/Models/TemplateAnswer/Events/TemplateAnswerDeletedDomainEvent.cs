using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Domain.Models.TemplateAnswer.Events;

public sealed record TemplateAnswerDeletedDomainEvent(TemplateAnswerId TemplateAnswerId) : IDomainEvent;