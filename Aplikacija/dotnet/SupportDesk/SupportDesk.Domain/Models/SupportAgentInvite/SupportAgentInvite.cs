using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.SupportAgentInvite.Enums;
using SupportDesk.Domain.Models.SupportAgentInvite.Events;
using SupportDesk.Domain.Models.SupportAgentInvite.Options;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite;

public sealed class SupportAgentInvite : AbstractDomainModel<SupportAgentInviteId>
{
    public SupportAgentInviteCode Code { get; private set; }
    public Email Email { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public SupportAgentInviteStatus Status { get; private set; }
    public CreatedAt CreatedAt { get; private set; }
    public SupportAgentInviteExpiresAt ExpiresAt { get; private set; }
    public SupportAgentInviteUsedAt? UsedAt { get; private set; }
    public SupportAgentInviteRevokedAt? RevokedAt { get; private set; }

    private SupportAgentInvite(
        SupportAgentInviteId id,
        SupportAgentInviteCode code,
        Email email,
        OrganizationId organizationId,
        SupportAgentInviteStatus status,
        CreatedAt createdAt,
        SupportAgentInviteExpiresAt expiresAt,
        SupportAgentInviteUsedAt? usedAt,
        SupportAgentInviteRevokedAt? revokedAt
        ) : base(id)
    {
        Code = code;
        Email = email;
        OrganizationId = organizationId;
        Status = status;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        UsedAt = usedAt;
        RevokedAt = revokedAt;
        
        ValidateModel();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new SupportAgentInviteCannotBeUsedBeforeItsCreatedRule(CreatedAt, UsedAt),
            new SupportAgentInviteCannotBeUsedAfterItsExpiredRule(ExpiresAt, UsedAt),
            new SupportAgentInviteCannotBeRevokedBeforeItsCreatedRule(CreatedAt, RevokedAt),
            new SupportAgentInviteCannotBeRevokedAfterItsExpiredRule(ExpiresAt, RevokedAt),
            new SupportAgentInviteStatusMustMatchTimestampsRule(Status, UsedAt, RevokedAt)
        ];
    }

    public SupportAgentInvite Create(string email, Guid organizationId)
    {
        var idVo = SupportAgentInviteId.NewId();
        var codeVo = new SupportAgentInviteCode(Guid.NewGuid());
        var emailVo = new Email(email);
        var organizationIdVo = new OrganizationId(organizationId);
        var status = SupportAgentInviteStatus.Active;
        var createdAtVo = new CreatedAt(DateTime.UtcNow);
        var expiresAtVo = new SupportAgentInviteExpiresAt(DateTime.UtcNow.AddDays(SupportAgentInviteOptionsDefaults.ExpirationDays));

        var createdSupportAgentInvite = new SupportAgentInvite(
            idVo,
            codeVo,
            emailVo,
            organizationIdVo,
            status,
            createdAtVo,
            expiresAtVo,
            null,
            null);
        
        createdSupportAgentInvite.RaiseDomainEvent(new SupportAgentInviteCreatedDomainEvent(createdSupportAgentInvite.Id));
        
        return createdSupportAgentInvite;       
    }

    private bool IsExpired()
    {
        return ExpiresAt.ExpiresAtValue < DateTime.UtcNow;
    }

    public void Use()
    {
        if (Status != SupportAgentInviteStatus.Active)
        {
            throw new ValidationException(SupportAgentInviteErrors.InvalidInviteCode(Code));
        }

        if (IsExpired())
        {
            throw new ValidationException(SupportAgentInviteErrors.ExpiredInviteCode(Code));       
        }

        Status = SupportAgentInviteStatus.Used;
        UsedAt = new SupportAgentInviteUsedAt(DateTime.UtcNow);
        
        RaiseDomainEvent(new SupportAgentInviteUsedDomainEvent(Id));       
    }
}