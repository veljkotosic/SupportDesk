namespace SupportDesk.Application.Abstract.ReadModels;

public interface IOrganizationReadModelService
{
    Task IndexOrganizationAsync(Guid id, string name, CancellationToken cancellationToken);
}
