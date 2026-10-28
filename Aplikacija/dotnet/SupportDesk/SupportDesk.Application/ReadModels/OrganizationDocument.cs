namespace SupportDesk.Application.ReadModels;

public sealed class OrganizationDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
