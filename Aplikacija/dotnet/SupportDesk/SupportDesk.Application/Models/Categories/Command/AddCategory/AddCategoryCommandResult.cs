using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Categories.Command.AddCategory;

public sealed record AddCategoryCommandResult(Guid CategoryId) : ICommandResult;