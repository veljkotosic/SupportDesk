using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

public sealed record SupportAgentInviteId(Guid IdValue) : DomainId(IdValue)
{
    public static SupportAgentInviteId NewId() => new(Guid.NewGuid());
}