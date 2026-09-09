using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs;

public sealed record GetOrganizationFaqsQuery(Guid OrganizationId) 
    : IQuery<GetOrganizationFaqsQueryResult>;