using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Domain.Models.Category.Events;

public record CategoryDeletedDomainEvent(CategoryId CategoryId) : IDomainEvent;