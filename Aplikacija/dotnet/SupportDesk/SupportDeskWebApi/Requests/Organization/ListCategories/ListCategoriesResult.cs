using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListCategories;

public record ListCategoriesResult(List<CategoryListingDto> Categories) : IRequestResult;