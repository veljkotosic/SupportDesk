using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Abstract.ReadModels;

public sealed record UserReadModelDto(
    Guid Id,
    Guid? OrganizationId,
    string UserName,
    string Email,
    UserRole Role
);
