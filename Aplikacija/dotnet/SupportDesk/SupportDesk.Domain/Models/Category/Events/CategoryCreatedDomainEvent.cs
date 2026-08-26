using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Domain.Models.Category.Events;

public sealed record CategoryCreatedDomainEvent(CategoryId CategoryId) : IDomainEvent;