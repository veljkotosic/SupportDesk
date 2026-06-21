using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Category.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Category.AddCategory;

public class AddCategoryRequestHandler 
    : IRequestHandler<AddCategoryRequest, AddCategoryResult>
{
    private readonly IUserContext _userContext;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCategoryRequestHandler(
        IUserContext userContext,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddCategoryResult> HandleAsync(AddCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var organizationId = _userContext.GetCurrentUsersOrganizationId()!;

        var category = await _categoryRepository.GetByNameAsync(request.Name, cancellationToken);
            
        if (category is not null)        
        {
            throw new Exception("Category with the same name already exists.");
        }

        category = new Data.Entities.Category.Category
        {
            Id = Guid.NewGuid(),
            OrganizationId = (Guid)organizationId,
            Name = request.Name,
            Description = request.Description
        };
            
        await _categoryRepository.SaveAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
            
        return new AddCategoryResult(category.Id);
    }
}