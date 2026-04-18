using System.Collections.Generic;
using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.UserAccess.Application.Configuration.Queries;

namespace HC.LIS.Modules.UserAccess.Application.Users.GetAuditLog;

internal class GetAuditLogQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetAuditLogQuery, IReadOnlyCollection<AuditLogEntryDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<AuditLogEntryDto>> Handle(GetAuditLogQuery query, CancellationToken cancellationToken)
    {
        const string sql = $"""
            SELECT
                a.id          AS "{nameof(AuditLogEntryDto.Id)}",
                a.occurred_at AS "{nameof(AuditLogEntryDto.OccurredAt)}",
                a.user_id     AS "{nameof(AuditLogEntryDto.UserId)}",
                a.actor_id    AS "{nameof(AuditLogEntryDto.ActorId)}",
                a.event_type  AS "{nameof(AuditLogEntryDto.EventType)}",
                a.details     AS "{nameof(AuditLogEntryDto.Details)}"
            FROM user_access.audit_log a
            WHERE (@UserId IS NULL OR a.user_id = @UserId)
              AND (@FromDate IS NULL OR a.occurred_at >= @FromDate)
              AND (@ToDate IS NULL OR a.occurred_at <= @ToDate)
            ORDER BY a.occurred_at DESC
            """;

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Database connection is unavailable.");

        IEnumerable<AuditLogEntryDto> results = await connection.QueryAsync<AuditLogEntryDto>(
            sql, new { query.UserId, query.FromDate, query.ToDate }
        ).ConfigureAwait(false);

        return results.AsList().AsReadOnly();
    }
}
