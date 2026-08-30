using SupportDesk.WebApi.Abstract.Request;

namespace SupportDesk.WebApi.Controllers.v1.Category.Requests;

public sealed record UpdateCategoryDetailsRequest(
    string? Name,
    string? Description
    ) : IAtLeastOneFieldRequiredRequest;