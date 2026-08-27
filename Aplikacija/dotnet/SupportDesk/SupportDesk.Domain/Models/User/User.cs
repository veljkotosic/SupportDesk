using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Events;
using SupportDesk.Domain.Models.User.Validation.Rules;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.User;

public sealed class User : AbstractDomainModel<UserId>
{
    public Email Email { get; private set; }
    public UserName UserName { get; private set; }
    public OrganizationId? OrganizationId { get; private set; }
    public UserRole Role { get; private set; } 
    public CreatedAt CreatedAt { get; private set; }

    internal User()
    {
        
    }
    
    private User(
        UserId id,
        Email email,
        UserName userName,
        OrganizationId? organizationId,
        UserRole role,
        CreatedAt createdAt
    ) : base(id)
    {
        Email = email;
        UserName = userName;
        OrganizationId = organizationId;
        Role = role;
        CreatedAt = createdAt;
        
        ValidateModel();       
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new CustomerCannotBePartOfOrganizationRule(OrganizationId, Role)
        ];
    }

    public static User Create(string email, string userName, Guid? organizationId, UserRole role, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;    
        
        var idVo = UserId.NewId();
        var emailVo = new Email(email);
        var userNameVo = new UserName(userName);
        var organizationIdVo = organizationId == null ? null : new OrganizationId((Guid)organizationId);
        var createdAtVo = new CreatedAt(now);
        
        var createdUser = new User(idVo, emailVo, userNameVo, organizationIdVo, role, createdAtVo);
        
        createdUser.RaiseDomainEvent(new UserCreatedDomainEvent(createdUser.Id.IdValue));
        
        return createdUser;
    }
}