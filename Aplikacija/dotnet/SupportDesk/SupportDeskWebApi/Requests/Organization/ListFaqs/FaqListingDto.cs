namespace SupportDeskWebApi.Requests.Organization.ListFaqs;

public record FaqListingDto(Guid Id, string Question, string Answer);
