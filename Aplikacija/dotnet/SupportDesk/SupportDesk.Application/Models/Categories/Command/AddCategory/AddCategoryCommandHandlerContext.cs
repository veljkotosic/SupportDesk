using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Categories.Command.AddCategory;

internal sealed record AddCategoryCommandHandlerContext(
    string Name, 
    Domain.Models.Category.Category? CategoryWithSameName
    ) : ICommandHandlerContext;