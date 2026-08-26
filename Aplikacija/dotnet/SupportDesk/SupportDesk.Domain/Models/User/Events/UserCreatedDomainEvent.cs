using SupportDesk.Domain.Abstract;

namespace SupportDesk.Domain.Models.User.Events;

public record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;