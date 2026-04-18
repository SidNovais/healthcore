using System.Collections.Generic;
using HC.Core.Application.Queries;
using HC.LIS.Modules.UserAccess.Application.Contracts;

namespace HC.LIS.Modules.UserAccess.Application.Users.GetUserList;

public class GetUserListQuery(int? page = null, int? perPage = null)
    : QueryBase<IReadOnlyCollection<UserListItemDto>>, IPagedQuery
{
    public int? Page { get; } = page;
    public int? PerPage { get; } = perPage;
}
