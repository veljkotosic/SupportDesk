using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Application.Models.Categories.Command.UpdateCategoryDetails;

internal sealed record UpdateCategoryDetailsCommandHandlerContext(
    Category? Category,
    CategoryId CategoryId,
    CategoryName? NewCategoryName,
    Category? CategoryWithSameName
    ) : ICommandHandlerContext;