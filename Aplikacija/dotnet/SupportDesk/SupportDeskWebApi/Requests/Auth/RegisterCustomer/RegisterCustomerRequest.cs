using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterCustomer;

public record RegisterCustomerRequest(
    string Username,
    string Email,
    string Password)
    : IRequest<RegisterCustomerResult>;