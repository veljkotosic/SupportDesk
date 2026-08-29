namespace SupportDesk.WebApi.Controllers.v1.Auth.Requests;

public sealed record RegisterCustomerRequest(string UserName, string Email, string Password);