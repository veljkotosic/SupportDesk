using Dapper;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Domain.Models.Organization.Events;

namespace SupportDesk.Application.Models.Organizations.Event.OrganizationCreated;

internal sealed class SyncOrganizationReadModelOnOrganizationCreatedDomainEventHandler
    : IDomainEventHandler<OrganizationCreatedDomainEvent>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IOrganizationReadModelService _organizationReadModelService;

    public SyncOrganizationReadModelOnOrganizationCreatedDomainEventHandler(
        IDbConnectionFactory dbConnectionFactory,
        IOrganizationReadModelService organizationReadModelService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _organizationReadModelService = organizationReadModelService;
    }

    public async Task HandleAsync(OrganizationCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT "Id", "Name"
            FROM "Organizations"
            WHERE "Id" = @OrganizationId;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var org = await connection.QuerySingleOrDefaultAsync<OrgRow>(sql, new { OrganizationId = domainEvent.OrganizationId.IdValue });

        if (org is null)
        {
            return;
        }

        await _organizationReadModelService.IndexOrganizationAsync(org.Id, org.Name, cancellationToken);
    }

    private sealed record OrgRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
