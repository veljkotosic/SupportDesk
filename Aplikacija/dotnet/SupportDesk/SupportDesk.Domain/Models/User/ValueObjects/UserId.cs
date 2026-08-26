using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.User.ValueObjects;

public sealed record UserId(Guid IdValue) : DomainId(IdValue)
{
    public static UserId NewId() => new(Guid.NewGuid());
}