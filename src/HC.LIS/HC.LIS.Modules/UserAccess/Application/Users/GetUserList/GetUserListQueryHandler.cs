using System.Collections.Generic;
using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.Core.Application.Queries;
using HC.LIS.Modules.UserAccess.Application.Configuration.Queries;

namespace HC.LIS.Modules.UserAccess.Application.Users.GetUserList;

internal class GetUserListQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetUserListQuery, IReadOnlyCollection<UserListItemDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<UserListItemDto>> Handle(GetUserListQuery query, CancellationToken cancellationToken)
    {
        const string baseSql = $"""
            SELECT
                u.id         AS "{nameof(UserListItemDto.Id)}",
                u.email      AS "{nameof(UserListItemDto.Email)}",
                u.full_name  AS "{nameof(UserListItemDto.FullName)}",
                u.role       AS "{nameof(UserListItemDto.Role)}",
                u.status     AS "{nameof(UserListItemDto.Status)}",
                u.created_at AS "{nameof(UserListItemDto.CreatedAt)}"
            FROM user_access.users u
            ORDER BY u.created_at DESC
            """;

        string sql = PagedQueryHelper.AppendPageStatement(baseSql);
        PageData pageData = PagedQueryHelper.GetPageData(query);

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Database connection is unavailable.");

        IEnumerable<UserListItemDto> results = await connection.QueryAsync<UserListItemDto>(
            sql, new { pageData.Offset, pageData.Next }
        ).ConfigureAwait(false);

        return results.AsList().AsReadOnly();
    }
}
