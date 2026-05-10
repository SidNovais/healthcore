using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Application.Users.Login;
using Microsoft.AspNetCore.Http;

namespace HC.LIS.API.Modules.UserAccess.Auth.Login;

internal static class LoginEndpoint
{
    internal static async Task<IResult> Handle(
        LoginRequest request,
        IUserAccessModule module,
        HttpContext httpContext,
        CancellationToken ct)
    {
        LoginResultDto result = await module.ExecuteCommandAsync(
            new LoginCommand(request.Email, request.Password)).ConfigureAwait(false);

        httpContext.Response.Cookies.Append("ACCESS_TOKEN", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return TypedResults.Ok(result);
    }
}
