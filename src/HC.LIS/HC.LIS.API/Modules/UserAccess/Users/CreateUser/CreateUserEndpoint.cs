using HC.Core.Application;
using HC.Core.Domain;
using HC.LIS.API.Common;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Application.Users.CreateUser;

namespace HC.LIS.API.Modules.UserAccess.Users.CreateUser;

internal static class CreateUserEndpoint
{
    internal static async Task<IResult> Handle(
        CreateUserRequest request,
        IUserAccessModule module,
        IExecutionContextAccessor executionContext,
        CancellationToken ct)
    {
        var userId = Guid.CreateVersion7();
        var invitationToken = Guid.CreateVersion7().ToString("N");

        await module.ExecuteCommandAsync(new CreateUserCommand(
            userId,
            request.Email,
            request.FullName,
            request.Birthdate,
            request.Gender,
            request.Role,
            invitationToken,
            SystemClock.Now,
            executionContext.UserId)).ConfigureAwait(false);

        return TypedResults.Created($"/api/v1/users/{userId}", new CreatedIdResponse(userId));
    }
}
