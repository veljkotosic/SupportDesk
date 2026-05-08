using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Category.RemoveCategory;

public record RemoveCategoryRequest(Guid CategoryId) : IRequest;