namespace SupportDesk.Application.Common.Auth;

public sealed record RefreshToken(string Value, Guid UserId, DateTime ExpiresAt);