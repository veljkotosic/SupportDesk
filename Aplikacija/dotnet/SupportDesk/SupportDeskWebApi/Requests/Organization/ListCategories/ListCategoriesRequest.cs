using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListCategories;

public record ListCategoriesRequest(Guid OrganizationId) : IRequest<ListCategoriesResult>;