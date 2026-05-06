namespace SupportDeskWebApi.Auth.Abstract;

public interface IUserContext
{
    Guid GetCurrentUserId();
}