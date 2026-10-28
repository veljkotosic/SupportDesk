namespace SupportDesk.Application.ReadModels;

public sealed class UserDocument
{
    public Guid Id { get; set; }
    public Guid? OrganizationId { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public short Role { get; set; }
}
