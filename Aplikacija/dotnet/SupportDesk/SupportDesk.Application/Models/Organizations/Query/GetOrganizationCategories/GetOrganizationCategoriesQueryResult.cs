using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationCategories.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationCategories;

public sealed record GetOrganizationCategoriesQueryResult(
    List<CategoryListingDto> Categories
    ) : IQueryResult;