namespace SupportDeskWebApi.Requests.Abstract;

public interface IRequest
{
    
}

public interface IRequest<TRequestResult>
    where TRequestResult : IRequestResult
{
    
}