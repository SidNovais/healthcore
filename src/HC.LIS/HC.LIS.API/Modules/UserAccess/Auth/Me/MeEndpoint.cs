using System.Security.Claims;
using HC.Core.Application;

namespace HC.LIS.API.Modules.UserAccess.Auth.CurrentUser;

internal static class MeEndpoint
{
    internal static IResult Handle(
        IExecutionContextAccessor executionContext,
        HttpContext httpContext)
    {
        string userName = httpContext.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        string role = httpContext.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return TypedResults.Ok(new MeResultDto(executionContext.UserId, userName, role));
    }
}
