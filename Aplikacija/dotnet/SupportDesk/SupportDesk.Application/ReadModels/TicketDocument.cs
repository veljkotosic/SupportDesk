namespace SupportDesk.Application.ReadModels;

public sealed class TicketDocument
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string CustomerUserName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public Guid? SupportAgentId { get; set; }
    public string? SupportAgentUserName { get; set; }
    public short Status { get; set; }
    public short Priority { get; set; }
    public short Feedback { get; set; }
    public string Subject { get; set; } = null!;
    public DateTime OpenedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public List<TicketNotificationDocument> UnreadNotifications { get; set; } = [];
}

public sealed class TicketNotificationDocument
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid TicketId { get; set; }
    public string Text { get; set; } = null!;
    public short Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
