namespace SupportDeskWebApi.Requests.Abstract;

public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<in TRequest, TRequestResult> 
    where TRequest : IRequest<TRequestResult>
    where TRequestResult : IRequestResult
{
    Task<TRequestResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}