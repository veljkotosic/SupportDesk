namespace SupportDesk.Application.Common.Auth;

public sealed record AccessToken(string Value, DateTime ExpiresAt);