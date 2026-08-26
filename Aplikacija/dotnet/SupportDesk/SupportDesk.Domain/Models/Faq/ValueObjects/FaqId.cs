using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.Faq.ValueObjects;

public sealed record FaqId(Guid IdValue) : DomainId(IdValue)
{
    public static FaqId NewId() => new(Guid.NewGuid());
}