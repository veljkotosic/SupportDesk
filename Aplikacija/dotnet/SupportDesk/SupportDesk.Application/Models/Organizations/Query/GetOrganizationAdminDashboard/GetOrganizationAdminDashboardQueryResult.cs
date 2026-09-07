using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;

public sealed record GetOrganizationAdminDashboardQueryResult(
    string OrganizationName,
    OrganizationAdminDashboardSummaryDto Summary,
    List<OrganizationAdminDashboardTicketVolumeEntryDto> TicketVolume,
    List<OrganizationAdminDashboardAgentDto> Agents,
    List<DashboardTicketDetailsDto> RecentTickets
    ) : IQueryResult;