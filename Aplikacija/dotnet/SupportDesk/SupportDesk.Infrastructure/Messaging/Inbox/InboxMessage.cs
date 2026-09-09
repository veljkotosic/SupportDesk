namespace SupportDesk.Infrastructure.Messaging.Inbox;

public sealed class InboxMessage
{
    public Guid Id { get; init; }
    public Guid OutboxMessageId { get; init; }
    public string HandlerType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public Guid? OrganizationId { get; init; }
    public DateTime ReceivedOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}