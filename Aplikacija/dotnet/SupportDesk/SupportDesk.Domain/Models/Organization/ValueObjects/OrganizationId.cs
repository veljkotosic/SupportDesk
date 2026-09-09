using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.Organization.ValueObjects;

public sealed record OrganizationId(Guid IdValue)
    : DomainId(IdValue)
{
    public static OrganizationId NewId() => new(Guid.NewGuid());
}