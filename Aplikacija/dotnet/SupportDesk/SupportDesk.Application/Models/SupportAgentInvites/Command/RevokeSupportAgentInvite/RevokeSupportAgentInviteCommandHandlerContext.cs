using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Application.Models.SupportAgentInvites.Command.RevokeSupportAgentInvite;

internal sealed record RevokeSupportAgentInviteCommandHandlerContext(
    SupportAgentInvite? SupportAgentInvite,
    SupportAgentInviteId SupportAgentInviteId
    ) : ICommandHandlerContext;