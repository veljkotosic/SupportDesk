using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Categories.Command.AddCategory;

public sealed record AddCategoryCommandHandlerContext(
    string Name, 
    Domain.Models.Category.Category? CategoryWithSameName
    ) : ICommandHandlerContext;