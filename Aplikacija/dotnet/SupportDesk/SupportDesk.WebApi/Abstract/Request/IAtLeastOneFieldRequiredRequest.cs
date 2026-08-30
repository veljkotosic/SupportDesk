namespace SupportDesk.WebApi.Abstract.Request;

public interface IAtLeastOneFieldRequiredRequest
{
    bool HasAnyFieldProvided()
    {
        return GetType()
            .GetProperties()
            .Any(p => p.GetValue(this) is not null);
    }
}