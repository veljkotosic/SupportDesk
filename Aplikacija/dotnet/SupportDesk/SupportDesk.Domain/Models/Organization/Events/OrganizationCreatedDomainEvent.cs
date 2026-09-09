using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Domain.Models.Organization.Events;

public sealed record OrganizationCreatedDomainEvent(OrganizationId OrganizationId) : IDomainEvent;