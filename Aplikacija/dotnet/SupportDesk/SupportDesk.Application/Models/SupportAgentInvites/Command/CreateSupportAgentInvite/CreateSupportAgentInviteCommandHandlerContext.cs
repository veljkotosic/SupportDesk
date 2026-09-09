using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.Application.Models.SupportAgentInvites.Command.CreateSupportAgentInvite;

internal sealed record CreateSupportAgentInviteCommandHandlerContext(
    User? ExistingUser,
    Email Email
    ) : ICommandHandlerContext;