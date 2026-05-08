using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Category.AddCategory;

public record AddCategoryRequest(
    string Name,
    string Description)
    : IRequest<AddCategoryResult>;