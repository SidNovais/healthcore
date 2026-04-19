using System.Data;
using Dapper;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.UserAccess.Application.Users;

namespace HC.LIS.Modules.UserAccess.Infrastructure.AuditLog;

internal class AuditLogWriter(ISqlConnectionFactory sqlConnectionFactory) : IAuditLogWriter
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task WriteAsync(Guid? userId, Guid? actorId, string eventType, string? details)
    {
        const string sql = """
            INSERT INTO user_access.audit_log (id, occurred_at, user_id, actor_id, event_type, details)
            VALUES (@Id, @OccurredAt, @UserId, @ActorId, @EventType, @Details)
            """;

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Database connection is unavailable.");

        await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            OccurredAt = SystemClock.Now,
            UserId = userId,
            ActorId = actorId,
            EventType = eventType,
            Details = details
        }).ConfigureAwait(false);
    }
}
