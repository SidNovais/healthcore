using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HC.LIS.API.Configuration.Authentication;

internal static class JwtCookieExtensions
{
    internal static IServiceCollection AddHcLisJwtCookieAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer = configuration["JWT_ISSUER"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_ISSUER");
        var audience = configuration["JWT_AUDIENCE"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_AUDIENCE");
        var secret = configuration["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_SECRET_KEY");
        var cookieName = configuration["JWT_COOKIE_NAME"] ?? "ACCESS_TOKEN";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secret))
                };

                // Extract token from HttpOnly cookie when Authorization header is absent
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Token) &&
                            context.Request.Cookies.TryGetValue(cookieName, out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
