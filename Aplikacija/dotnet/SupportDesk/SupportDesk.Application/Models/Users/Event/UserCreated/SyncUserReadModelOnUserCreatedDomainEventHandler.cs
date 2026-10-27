using Dapper;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Events;

namespace SupportDesk.Application.Models.Users.Event.UserCreated;

internal sealed class SyncUserReadModelOnUserCreatedDomainEventHandler
    : IDomainEventHandler<UserCreatedDomainEvent>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUserReadModelService _userReadModelService;

    public SyncUserReadModelOnUserCreatedDomainEventHandler(
        IDbConnectionFactory dbConnectionFactory,
        IUserReadModelService userReadModelService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _userReadModelService = userReadModelService;
    }

    public async Task HandleAsync(UserCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT "Id", "OrganizationId", "UserName", "Email", "Role"
            FROM "Users"
            WHERE "Id" = @UserId;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var user = await connection.QuerySingleOrDefaultAsync<UserRow>(sql, new { UserId = domainEvent.UserId });

        if (user is null)
        {
            return;
        }

        var dto = new UserReadModelDto(
            user.Id,
            user.OrganizationId,
            user.UserName,
            user.Email,
            (UserRole)user.Role);

        await _userReadModelService.IndexUserAsync(dto, cancellationToken);
    }

    private sealed record UserRow
    {
        public Guid Id { get; set; }
        public Guid? OrganizationId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public short Role { get; set; }
    }
}
