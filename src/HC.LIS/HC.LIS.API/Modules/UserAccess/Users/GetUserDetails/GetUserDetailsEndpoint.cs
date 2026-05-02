using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Application.Users.GetUserDetails;

namespace HC.LIS.API.Modules.UserAccess.Users.GetUserDetails;

internal static class GetUserDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid userId,
        IUserAccessModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetUserDetailsQuery(userId)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
