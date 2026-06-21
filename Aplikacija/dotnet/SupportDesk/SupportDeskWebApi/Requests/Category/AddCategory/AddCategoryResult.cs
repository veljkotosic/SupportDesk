using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Category.AddCategory;

public record AddCategoryResult(Guid CategoryId) : IRequestResult;