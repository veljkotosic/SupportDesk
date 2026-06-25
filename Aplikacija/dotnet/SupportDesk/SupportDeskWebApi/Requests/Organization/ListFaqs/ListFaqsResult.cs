using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListFaqs;

public record ListFaqsResult(List<FaqListingDto> Faqs) : IRequestResult;
