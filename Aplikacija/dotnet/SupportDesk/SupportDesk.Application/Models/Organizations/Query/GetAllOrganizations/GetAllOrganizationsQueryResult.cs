using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetAllOrganizations.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetAllOrganizations;

public sealed record GetAllOrganizationsQueryResult(
    List<OrganizationListingDto> Organizations
    ) : IQueryResult;