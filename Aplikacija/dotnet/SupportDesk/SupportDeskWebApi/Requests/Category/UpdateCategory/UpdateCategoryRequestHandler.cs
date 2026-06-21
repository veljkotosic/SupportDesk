using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Category.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Category.UpdateCategory;

public class UpdateCategoryRequestHandler 
    : IRequestHandler<UpdateCategoryRequest>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryRequestHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        
        if (category is null) 
        {
            throw new InvalidOperationException("Category not found.");
        }
        
        var categoryWithSameName = await _categoryRepository.GetByNameAsync(request.Name, cancellationToken);
        
        if (categoryWithSameName is not null && categoryWithSameName.Id != category.Id)
        {
            throw new InvalidOperationException("Category with the same name already exists.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        
        await _categoryRepository.SaveAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
