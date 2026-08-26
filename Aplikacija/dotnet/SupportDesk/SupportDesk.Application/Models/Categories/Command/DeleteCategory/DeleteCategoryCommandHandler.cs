using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Category.Repository;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Application.Models.Categories.Command.DeleteCategory;

public class DeleteCategoryCommandHandler
    : AbstractCommandHandler<DeleteCategoryCommand, DeleteCategoryCommandHandlerContext>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        PermissionChecker permissionChecker, 
        ICategoryRepository categoryRepository, 
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<DeleteCategoryCommandHandlerContext> PrepareAsync(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoryId = new CategoryId(command.CategoryId);
        
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        
        return new DeleteCategoryCommandHandlerContext(categoryId, category);
    }

    protected override async Task HandleInternalAsync(DeleteCategoryCommand command, DeleteCategoryCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var category = context.Category!;
        
        category.Delete();
        
        await _categoryRepository.SaveAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(DeleteCategoryCommandHandlerContext context)
    {
        return [
            [
                new DomainModelExistsRule<Domain.Models.Category.Category, CategoryId>(context.Category, context.Id)
            ]
        ];
    }
}