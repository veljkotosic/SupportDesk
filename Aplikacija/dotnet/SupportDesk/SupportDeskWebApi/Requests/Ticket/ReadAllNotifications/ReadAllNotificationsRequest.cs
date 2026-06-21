using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.ReadAllNotifications;

public record ReadAllNotificationsRequest(Guid TicketId) : IRequest;