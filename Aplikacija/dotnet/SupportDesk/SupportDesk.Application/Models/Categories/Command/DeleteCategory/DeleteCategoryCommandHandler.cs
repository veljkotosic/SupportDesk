using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.Repository;
using SupportDesk.Domain.Models.Category.Validation.Rules;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Application.Models.Categories.Command.DeleteCategory;

internal class DeleteCategoryCommandHandler
    : AbstractCommandHandler<DeleteCategoryCommand, DeleteCategoryCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        PermissionChecker permissionChecker, 
        TimeProvider timeProvider,
        ICategoryRepository categoryRepository, 
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<DeleteCategoryCommandHandlerContext> PrepareAsync(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoryId = new CategoryId(command.CategoryId);
        
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        
        return new DeleteCategoryCommandHandlerContext(categoryId, category);
    }

    protected override async Task ExecuteAsync(DeleteCategoryCommand command, DeleteCategoryCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var category = context.Category!;
        
        category.Delete(_timeProvider);
        
        await _categoryRepository.SaveAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(DeleteCategoryCommandHandlerContext context)
    {
        return [
            [
                new DomainModelExistsRule<Category, CategoryId>(context.Category, context.Id)
            ],
            [
                new CannotDeleteAlreadyDeletedCategoryRule(context.Category!)
            ]
        ];
    }
}