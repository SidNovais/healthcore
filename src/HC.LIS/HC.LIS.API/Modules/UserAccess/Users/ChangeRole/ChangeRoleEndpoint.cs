using HC.Core.Application;
using HC.Core.Domain;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Application.Users.ChangeRole;

namespace HC.LIS.API.Modules.UserAccess.Users.ChangeRole;

internal static class ChangeRoleEndpoint
{
    internal static async Task<IResult> Handle(
        Guid userId,
        ChangeRoleRequest request,
        IUserAccessModule module,
        IExecutionContextAccessor executionContext,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new ChangeRoleCommand(
            userId,
            request.NewRole,
            executionContext.UserId,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
