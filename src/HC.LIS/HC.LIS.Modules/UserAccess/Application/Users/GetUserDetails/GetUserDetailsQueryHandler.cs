using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.UserAccess.Application.Configuration.Queries;

namespace HC.LIS.Modules.UserAccess.Application.Users.GetUserDetails;

internal class GetUserDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetUserDetailsQuery, UserDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<UserDetailsDto?> Handle(GetUserDetailsQuery query, CancellationToken cancellationToken)
    {
        const string sql = $"""
            SELECT
                u.id            AS "{nameof(UserDetailsDto.Id)}",
                u.email         AS "{nameof(UserDetailsDto.Email)}",
                u.full_name     AS "{nameof(UserDetailsDto.FullName)}",
                u.birthdate     AS "{nameof(UserDetailsDto.Birthdate)}",
                u.gender        AS "{nameof(UserDetailsDto.Gender)}",
                u.role          AS "{nameof(UserDetailsDto.Role)}",
                u.status        AS "{nameof(UserDetailsDto.Status)}",
                u.created_at    AS "{nameof(UserDetailsDto.CreatedAt)}",
                u.created_by_id AS "{nameof(UserDetailsDto.CreatedById)}",
                u.activated_at  AS "{nameof(UserDetailsDto.ActivatedAt)}"
            FROM user_access.users u
            WHERE (@UserId IS NULL OR u.id = @UserId)
              AND (@Email IS NULL OR u.email = @Email)
            """;

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Database connection is unavailable.");

        return await connection.QueryFirstOrDefaultAsync<UserDetailsDto>(
            sql, new { query.UserId, query.Email }
        ).ConfigureAwait(false);
    }
}
