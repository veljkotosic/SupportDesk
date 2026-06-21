using SupportDeskWebApi.Data.Entities.Category.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Category.RemoveCategory;

public class RemoveCategoryRequestHandler
    : IRequestHandler<RemoveCategoryRequest>
{
    private readonly ICategoryRepository _categoryRepository;

    public RemoveCategoryRequestHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task HandleAsync(RemoveCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new Exception("Category not found");
        }
        
        await _categoryRepository.DeleteAsync(category, cancellationToken);
    }
}