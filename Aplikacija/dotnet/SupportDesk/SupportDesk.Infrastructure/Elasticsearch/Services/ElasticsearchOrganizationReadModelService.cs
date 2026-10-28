using Elastic.Clients.Elasticsearch;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Application.ReadModels;

namespace SupportDesk.Infrastructure.Elasticsearch.Services;

public sealed class ElasticsearchOrganizationReadModelService : IOrganizationReadModelService
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchOrganizationReadModelService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task IndexOrganizationAsync(Guid id, string name, CancellationToken cancellationToken)
    {
        var document = new OrganizationDocument
        {
            Id = id,
            Name = name
        };

        await _client.IndexAsync(document, idx => idx
            .Index(ElasticsearchIndices.Organizations)
            .Id(document.Id.ToString()), cancellationToken);
    }
}
