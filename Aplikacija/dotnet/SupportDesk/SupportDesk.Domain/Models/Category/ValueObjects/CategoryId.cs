using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.Category.ValueObjects;

public sealed record CategoryId(Guid IdValue) : DomainId(IdValue)
{
    public static CategoryId NewId() => new(Guid.NewGuid());
}