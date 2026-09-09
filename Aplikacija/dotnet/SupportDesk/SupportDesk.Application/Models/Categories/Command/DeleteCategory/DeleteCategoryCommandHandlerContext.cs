using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Application.Models.Categories.Command.DeleteCategory;

internal sealed record DeleteCategoryCommandHandlerContext(CategoryId Id, Domain.Models.Category.Category? Category) : ICommandHandlerContext;