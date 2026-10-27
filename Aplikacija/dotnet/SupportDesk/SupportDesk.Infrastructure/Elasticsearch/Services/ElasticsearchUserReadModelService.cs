using Elastic.Clients.Elasticsearch;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Infrastructure.Elasticsearch.Documents;

namespace SupportDesk.Infrastructure.Elasticsearch.Services;

public sealed class ElasticsearchUserReadModelService : IUserReadModelService
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchUserReadModelService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task IndexUserAsync(UserReadModelDto user, CancellationToken cancellationToken)
    {
        var document = new UserDocument
        {
            Id = user.Id,
            OrganizationId = user.OrganizationId,
            UserName = user.UserName,
            Email = user.Email,
            Role = (short)user.Role
        };

        await _client.IndexAsync(document, idx => idx
            .Index(ElasticsearchIndices.Users)
            .Id(document.Id.ToString()), cancellationToken);
    }
}
