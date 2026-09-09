using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Organization.Enums;
using SupportDesk.Domain.Models.Organization.Events;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Domain.Models.Organization;

public sealed class Organization : AbstractDomainModel<OrganizationId>
{
    public OrganizationName Name { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public CreatedAt CreatedAt { get; private set; }
    public DeletedAt? DeletedAt { get; private set; }

    internal Organization()
    {
        
    }
    
    private Organization(
        OrganizationId id,
        OrganizationName name,
        OrganizationStatus status,
        CreatedAt createdAt,
        DeletedAt? deletedAt = null
        ) : base(id)
    {
        Name = name;
        Status = status;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;
    }

    public static Organization Create(string name, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;     
        
        var idVo = OrganizationId.NewId();
        var nameVo = new OrganizationName(name);
        var statusVo = OrganizationStatus.Active;
        var createdAtVo = new CreatedAt(now);
        
        var createdOrganization = new Organization(idVo, nameVo, statusVo, createdAtVo);
        
        createdOrganization.RaiseDomainEvent(new OrganizationCreatedDomainEvent(createdOrganization.Id));
        
        return createdOrganization;
    }
}