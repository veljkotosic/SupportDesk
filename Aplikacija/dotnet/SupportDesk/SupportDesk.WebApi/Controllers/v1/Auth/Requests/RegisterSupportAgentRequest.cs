namespace SupportDesk.WebApi.Controllers.v1.Auth.Requests;

public sealed record RegisterSupportAgentRequest(string UserName, string Email, string Password, Guid Code);