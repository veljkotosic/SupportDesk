using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Domain.Models.TemplateAnswer.Events;

public sealed record TemplateAnswerCreatedDomainEvent(TemplateAnswerId TemplateAnswerId) : IDomainEvent;