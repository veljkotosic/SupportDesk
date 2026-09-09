namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase.Dtos;

public sealed record KnowledgeBaseFaqDto(Guid Id, string Question, string Answer);