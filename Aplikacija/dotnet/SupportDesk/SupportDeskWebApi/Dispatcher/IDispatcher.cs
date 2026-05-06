using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Dispatcher;

public interface IDispatcher
{
    Task ExecuteAsync(IRequest request, CancellationToken cancellationToken = default);
    Task<TRequestResult> ExecuteAsync<TRequestResult>(IRequest<TRequestResult> request, CancellationToken cancellationToken = default)
        where TRequestResult : IRequestResult;
    
    
}