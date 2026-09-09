using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Category.Events;
using SupportDesk.Domain.Models.Category.Validation;
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

    internal Category()
    {
        
    }
    
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

    public static Category Create(Guid organizationId, string name, string description, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;      
        
        var idVo = CategoryId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var nameVo = new CategoryName(name);
        var descriptionVo = new CategoryDescription(description);
        var createdAtVo = new CreatedAt(now);       

        var createdCategory = new Category(idVo, organizationIdVo, nameVo, descriptionVo, createdAtVo, null);
        
        createdCategory.RaiseDomainEvent(new CategoryCreatedDomainEvent(createdCategory.Id));
        
        return createdCategory;
    }

    public void Delete(TimeProvider timeProvider)
    {
        if (DeletedAt is not null)
        {
            throw new ValidationException(CategoryErrors.AlreadyDeleted(Name));
        }
        
        var now = timeProvider.GetUtcNow().UtcDateTime;
        
        DeletedAt = new DeletedAt(now);
        
        RaiseDomainEvent(new CategoryDeletedDomainEvent(Id));
    }

    public void UpdateDetails(string? name, string? description)
    {
        var hasChanged = false;

        if (name is not null)
        {
            var newName = new CategoryName(name);
            if (newName != Name) 
            {
                Name = newName;
                hasChanged = true;
            }
        }

        if (description is not null)
        {
            var newDescription = new CategoryDescription(description);
            if (newDescription != Description) 
            {
                Description = newDescription;
                hasChanged = true;
            }
        }

        if (hasChanged)
        {
            RaiseDomainEvent(new CategoryDetailsUpdatedDomainEvent(Id, Name, Description));
        }
    }
}