using SupportDesk.Domain.Abstract;

namespace SupportDesk.Domain.Models.User.Events;

public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;