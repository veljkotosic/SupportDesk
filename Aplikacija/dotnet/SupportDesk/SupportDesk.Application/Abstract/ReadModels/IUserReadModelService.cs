namespace SupportDesk.Application.Abstract.ReadModels;

public interface IUserReadModelService
{
    Task IndexUserAsync(UserReadModelDto user, CancellationToken cancellationToken);
}
