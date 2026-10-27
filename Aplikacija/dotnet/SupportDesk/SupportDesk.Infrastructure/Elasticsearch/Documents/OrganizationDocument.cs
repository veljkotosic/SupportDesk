namespace SupportDesk.Infrastructure.Elasticsearch.Documents;

public sealed class OrganizationDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
