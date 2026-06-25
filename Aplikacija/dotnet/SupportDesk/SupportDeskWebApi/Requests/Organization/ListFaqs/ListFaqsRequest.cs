using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListFaqs;

public record ListFaqsRequest(Guid OrganizationId) : IRequest<ListFaqsResult>;
