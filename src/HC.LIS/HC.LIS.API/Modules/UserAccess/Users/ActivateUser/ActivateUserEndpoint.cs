using HC.Core.Domain;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Application.Users.ActivateUser;

namespace HC.LIS.API.Modules.UserAccess.Users.ActivateUser;

internal static class ActivateUserEndpoint
{
    internal static async Task<IResult> Handle(
        Guid userId,
        ActivateUserRequest request,
        IUserAccessModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new ActivateUserCommand(
            userId,
            request.InvitationToken,
            request.Password,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
