namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs.Dtos;

public sealed record FaqListingDto(Guid Id, string Question, string Answer);