using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationCategories;

public sealed record GetOrganizationCategoriesQuery(Guid OrganizationId) 
    : IQuery<GetOrganizationCategoriesQueryResult>;