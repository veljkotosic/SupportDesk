using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Category.Events;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Domain.Models.Category;

public sealed class Category : AbstractDomainModel<CategoryId>
{
    public OrganizationId OrganizationId { get; private set; }
    public CategoryName Name { get; private set; }
    public CategoryDescription Description { get; private set; }
    public CreatedAt CreatedAt { get; private set; }
    public DeletedAt? DeletedAt { get; private set; }
    
    private Category(
        CategoryId id,
        OrganizationId organizationId,
        CategoryName name,
        CategoryDescription description,
        CreatedAt createdAt,
        DeletedAt? deletedAt
    ) : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;
    }

    public static Category Create(Guid organizationId, string name, string description)
    {
        var idVo = CategoryId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var nameVo = new CategoryName(name);
        var descriptionVo = new CategoryDescription(description);
        var createdAtVo = new CreatedAt(DateTime.UtcNow);       

        var createdCategory = new Category(idVo, organizationIdVo, nameVo, descriptionVo, createdAtVo, null);
        
        createdCategory.RaiseDomainEvent(new CategoryCreatedDomainEvent(createdCategory.Id));
        
        return createdCategory;
    }

    public void Delete()
    {
        DeletedAt = new DeletedAt(DateTime.UtcNow);
        
        RaiseDomainEvent(new CategoryDeletedDomainEvent(Id));
    }
}