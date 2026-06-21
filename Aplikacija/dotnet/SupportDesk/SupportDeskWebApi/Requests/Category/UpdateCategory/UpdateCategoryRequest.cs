using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Category.UpdateCategory;

public record UpdateCategoryRequest(Guid CategoryId, string Name, string Description) : IRequest;
