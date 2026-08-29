namespace SupportDesk.WebApi.Controllers.v1.Auth.Requests;

public sealed record LoginRequest(string Email, string Password);