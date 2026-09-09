using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.Repository;
using SupportDesk.Domain.Models.Category.Validation.Rules;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Application.Models.Categories.Command.UpdateCategoryDetails;

internal sealed class UpdateCategoryDetailsCommandHandler
    : AbstractCommandHandler<UpdateCategoryDetailsCommand, UpdateCategoryDetailsCommandHandlerContext>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public UpdateCategoryDetailsCommandHandler(PermissionChecker permissionChecker, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<UpdateCategoryDetailsCommandHandlerContext> PrepareAsync(UpdateCategoryDetailsCommand command, CancellationToken cancellationToken)
    {
        var categoryId = new CategoryId(command.CategoryId);

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (command.Name == null)
        {
            return new UpdateCategoryDetailsCommandHandlerContext(category, categoryId, null, null);
        }
        
        var newCategoryName = new CategoryName(command.Name);
            
        var categoryWithSameName = await _categoryRepository.GetByNameAsync(newCategoryName, cancellationToken);
            
        return new UpdateCategoryDetailsCommandHandlerContext(category, categoryId, newCategoryName, categoryWithSameName);

    }

    protected override async Task ExecuteAsync(UpdateCategoryDetailsCommand command, UpdateCategoryDetailsCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var category = context.Category!;
        category.UpdateDetails(command.Name, command.Description);
        
        await _categoryRepository.SaveAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(UpdateCategoryDetailsCommandHandlerContext context)
    {
        return 
        [
            [
                new DomainModelExistsRule<Category, CategoryId>(context.Category, context.CategoryId)
            ],
            [
                new CannotUpdateCategoryNameWithExistingOneRule(context.Category!, context.NewCategoryName, context.CategoryWithSameName)
            ]
        ];
    }
}