using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;

namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetDashboard;

public record GetDashboardResult(
    string OrganizationName,
    DashboardSummaryDto Summary,
    List<TicketVolumeEntryDto> TicketVolume,
    List<DashboardAgentDto> Agents,
    List<TicketDetailsDto> RecentTickets) 
    : IRequestResult;
