using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary;

public sealed record GetOrganizationSupportAgentsSummaryQueryResult(
    List<SupportAgentSummary> Agents
    ) : IQueryResult;