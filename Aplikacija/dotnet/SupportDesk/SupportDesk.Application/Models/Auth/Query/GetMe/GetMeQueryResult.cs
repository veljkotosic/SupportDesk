using SupportDesk.Application.Abstract.Query;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Models.Auth.Query.GetMe;

public sealed record GetMeQueryResult(Guid UserId, string UserName, string Email, UserRole Role) : IQueryResult;