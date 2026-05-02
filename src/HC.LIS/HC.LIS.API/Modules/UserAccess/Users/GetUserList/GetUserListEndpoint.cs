using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Application.Users.GetUserList;

namespace HC.LIS.API.Modules.UserAccess.Users.GetUserList;

internal static class GetUserListEndpoint
{
    internal static async Task<IResult> Handle(
        int? page,
        int? perPage,
        IUserAccessModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetUserListQuery(page, perPage)).ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
