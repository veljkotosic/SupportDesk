using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.Repository;
using SupportDesk.Domain.Models.Category.Validation.Rules;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Application.Models.Categories.Command.AddCategory;

internal sealed class AddCategoryCommandHandler
    : AbstractCommandHandler<AddCategoryCommand, AddCategoryCommandResult, AddCategoryCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITenantContext _tenantContext;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCategoryCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITenantContext tenantContext,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tenantContext = tenantContext;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<AddCategoryCommandHandlerContext> PrepareAsync(AddCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoryName = new CategoryName(command.Name);
        
        var categoryWithSameName = await _categoryRepository.GetByNameAsync(categoryName, cancellationToken);
        
        return new AddCategoryCommandHandlerContext(command.Name, categoryWithSameName);
    }

    protected override async Task<AddCategoryCommandResult> ExecuteAsync(AddCategoryCommand command, AddCategoryCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;
        
        var category = Category.Create(organizationId, command.Name, command.Description, _timeProvider);

        await _categoryRepository.SaveAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new AddCategoryCommandResult(category.Id.IdValue);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(AddCategoryCommandHandlerContext context)
    {
        return [
            [
                new CannotCreateCategoryWithExistingNameRule(context.Name, context.CategoryWithSameName)
            ]
        ];
    }
}