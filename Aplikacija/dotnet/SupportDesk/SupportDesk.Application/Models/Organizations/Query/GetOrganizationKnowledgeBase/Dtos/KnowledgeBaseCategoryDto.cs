namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase.Dtos;

public sealed record KnowledgeBaseCategoryDto(Guid Id, string Name, string Description, int TicketCount);