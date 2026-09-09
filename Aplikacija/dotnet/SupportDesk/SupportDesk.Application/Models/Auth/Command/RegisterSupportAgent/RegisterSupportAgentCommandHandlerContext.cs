using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Application.Models.Auth.Command.RegisterSupportAgent;

internal sealed record RegisterSupportAgentCommandHandlerContext(
    SupportAgentInvite? SupportAgentInvite,
    SupportAgentInviteCode Code,
    Email Email
    ) : ICommandHandlerContext;