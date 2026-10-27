using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;
using SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;
using SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Abstract.ReadModels;

public interface ITicketReadModelService
{
    // Queries
    Task<GetCustomerTicketsQueryResult> GetCustomerTicketsAsync(
        GetCustomerTicketsQuery query,
        Guid customerId,
        CancellationToken cancellationToken);

    Task<GetOrganizationTicketsQueryResult> GetOrganizationTicketsAsync(
        GetOrganizationTicketsQuery query,
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<GetOrganizationAdminDashboardQueryResult> GetOrganizationAdminDashboardAsync(
        GetOrganizationAdminDashboardQuery query,
        Guid organizationId,
        TimeProvider timeProvider,
        CancellationToken cancellationToken);

    // Projections
    Task IndexTicketAsync(TicketReadModelDto ticket, CancellationToken cancellationToken);

    Task UpdateTicketAssignmentAsync(
        Guid ticketId,
        Guid? supportAgentId,
        string? supportAgentUserName,
        TicketStatus status,
        DateTime? assignedAt,
        CancellationToken cancellationToken);

    Task UpdateTicketStatusAsync(
        Guid ticketId,
        TicketStatus status,
        DateTime? closedAt,
        CancellationToken cancellationToken);

    Task UpdateTicketFeedbackAsync(
        Guid ticketId,
        TicketFeedback feedback,
        CancellationToken cancellationToken);

    Task UpdateTicketLastMessageAtAsync(
        Guid ticketId,
        DateTime lastMessageAt,
        CancellationToken cancellationToken);

    Task AddUnreadNotificationAsync(
        Guid ticketId,
        TicketNotificationDetailsDto notification,
        CancellationToken cancellationToken);

    Task RemoveUnreadNotificationAsync(
        Guid ticketId,
        Guid notificationId,
        CancellationToken cancellationToken);
}
