using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Command.OpenTicket;

internal sealed record OpenTicketCommandHandlerContext(
    Organization? Organization,
    OrganizationId OrganizationId,
    Category? Category,
    CategoryId CategoryId
    ) : ICommandHandlerContext;