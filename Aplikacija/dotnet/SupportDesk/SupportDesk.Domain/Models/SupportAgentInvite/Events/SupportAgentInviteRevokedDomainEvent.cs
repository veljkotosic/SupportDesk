using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Events;

public sealed record SupportAgentInviteRevokedDomainEvent(SupportAgentInviteId SupportAgentInviteId) : IDomainEvent;