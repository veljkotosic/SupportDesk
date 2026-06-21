using SupportDeskWebApi.Data.Entities.Category.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListCategories;

public class ListCategoriesRequestHandler
    : IRequestHandler<ListCategoriesRequest, ListCategoriesResult>
{
    private readonly ICategoryRepository _categoryRepository;

    public ListCategoriesRequestHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ListCategoriesResult> HandleAsync(ListCategoriesRequest request, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetCategoriesByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        
        return new ListCategoriesResult(categories);       
    }
}