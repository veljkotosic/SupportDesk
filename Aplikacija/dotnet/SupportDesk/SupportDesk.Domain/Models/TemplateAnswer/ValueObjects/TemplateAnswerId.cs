using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

public sealed record TemplateAnswerId(Guid IdValue) : DomainId(IdValue)
{
    public static TemplateAnswerId NewId() => new(Guid.NewGuid());
}