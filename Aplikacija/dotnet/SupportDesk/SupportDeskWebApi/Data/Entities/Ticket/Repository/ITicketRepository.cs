using SupportDeskWebApi.Data.Entities.Common.Repository;
using SupportDeskWebApi.Requests.Ticket.Common;
using SupportDeskWebApi.Requests.Ticket.GetTicketViewInfo;

namespace SupportDeskWebApi.Data.Entities.Ticket.Repository;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<List<TicketDetailsDto>> GetCustomerTicketsAsync(CancellationToken cancellationToken = default);
    Task<TicketDetailsDto?> GetTicketAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TicketViewInfoDto?> GetTicketViewInfoAsync(Guid id, CancellationToken cancellationToken = default);
}