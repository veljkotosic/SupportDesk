namespace SupportDesk.WebApi.Controllers.v1.Auth.Requests;

public sealed record RegisterOrganizationRequest(
    string UserName,
    string OrganizationName,
    string Email,
    string Password);