using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.UserAccess.Application.Configuration.Queries;

namespace HC.LIS.Modules.UserAccess.Application.Users.Login;

internal class GetUserByEmailQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetUserByEmailQuery, UserAuthDataDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<UserAuthDataDto?> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        const string sql = $"""
            SELECT
                u.id         AS "{nameof(UserAuthDataDto.Id)}",
                u.email      AS "{nameof(UserAuthDataDto.Email)}",
                u.role       AS "{nameof(UserAuthDataDto.Role)}",
                u.password_hash AS "{nameof(UserAuthDataDto.PasswordHash)}"
            FROM user_access.users u
            WHERE u.email = @Email
            """;

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Database connection is unavailable.");

        return await connection.QueryFirstOrDefaultAsync<UserAuthDataDto>(
            sql, new { query.Email }
        ).ConfigureAwait(false);
    }
}
