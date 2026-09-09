using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.Application.Models.Messages.Query.GetMessageDetails;

public sealed record GetMessageDetailsQueryResult(
    Guid Id,
    Guid SenderId,
    string SenderUsername,
    string Text,
    DateTime CreatedAt
    ) : IQueryResult;