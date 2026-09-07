using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs;

public sealed record GetOrganizationFaqsQueryResult(List<FaqListingDto> Faqs) : IQueryResult;