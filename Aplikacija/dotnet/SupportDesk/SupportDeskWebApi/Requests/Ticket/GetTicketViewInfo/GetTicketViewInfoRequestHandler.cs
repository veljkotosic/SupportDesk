using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Message.Common;
using SupportDeskWebApi.Requests.Note.Common;

namespace SupportDeskWebApi.Requests.Ticket.GetTicketViewInfo;

public class GetTicketViewInfoRequestHandler
    : IRequestHandler<GetTicketViewInfoRequest, GetTicketViewInfoResult>
{
    private readonly IUserContext _userContext;
    private readonly SupportDeskDbContext _context;

    public GetTicketViewInfoRequestHandler(
        IUserContext userContext,
        SupportDeskDbContext context)
    {
        _userContext = userContext;
        _context = context;
    }

    public async Task<GetTicketViewInfoResult> HandleAsync(GetTicketViewInfoRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var ticket = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == request.TicketId)
            .Select(t => new TicketViewInfoDto(
                t.Id,
                t.OrganizationId,
                t.Organization.Name,
                t.CategoryId,
                t.Category.Name,
                t.CustomerId,
                t.Customer.UserName!,
                t.SupportAgentId,
                t.SupportAgent != null ? t.SupportAgent.UserName : null,
                t.Status,
                t.Priority,
                t.Subject,
                t.OpenedAt,
                t.AssignedAt,
                t.ClosedAt,
                t.Feedback,
                t.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new MessageDetailsDto(
                        m.Id,
                        m.SenderId,
                        m.Sender.UserName!,
                        m.Text,
                        m.CreatedAt))
                    .ToList(),
                t.Notes
                    .Select(n => new NoteDetailsDto(
                        n.Id,
                        n.AuthorId,
                        n.Author.UserName!,
                        n.Text,
                        n.CreatedAt))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket is null)
        {
            throw new Exception("Ticket not found.");      
        }

        if (ticket.CustomerId != userId)
        {
            throw new UnauthorizedAccessException("You can only view your own tickets.");     
        }
        
        return new GetTicketViewInfoResult(ticket);      
    }
}
