using HC.LIS.API.Modules.UserAccess.Auth.Login;
using HC.LIS.Modules.UserAccess.Application.Users.Login;

namespace HC.LIS.API.Modules.UserAccess.Auth;

internal static class AuthEndpoints
{
    internal static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.WithTags("Auth");

        group.MapPost("login", LoginEndpoint.Handle)
            .WithName("Login")
            .WithSummary("Authenticate and receive a JWT token.")
            .Produces<LoginResultDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
