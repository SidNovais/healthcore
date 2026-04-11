# Template: JWT Authentication Extensions

Two variants — generate only the one chosen in Q&A.

---

## Variant A: JWT Bearer only (standard Authorization header)

**Output path:** `src/HC.LIS/HC.LIS.API/Configuration/Authentication/JwtExtensions.cs`

Configuration keys read (with `ASPNETCORE_HCLIS_` prefix stripped):
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `JWT_SECRET_KEY`

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HC.LIS.API.Configuration.Authentication;

internal static class JwtExtensions
{
    internal static IServiceCollection AddHcLisJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer   = configuration["JWT_ISSUER"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_ISSUER");
        var audience = configuration["JWT_AUDIENCE"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_AUDIENCE");
        var secret   = configuration["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_SECRET_KEY");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey  = true,
                    ValidIssuer              = issuer,
                    ValidAudience            = audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(secret))
                };
            });

        return services;
    }
}
```

Also add in `ConfigureServices`:
```csharp
services.AddHcLisJwtAuthentication(_configuration);
```

---

## Variant B: JWT stored in HttpOnly cookie

**Output path:** `src/HC.LIS/HC.LIS.API/Configuration/Authentication/JwtCookieExtensions.cs`

Same configuration keys as Variant A plus optional:
- `JWT_COOKIE_NAME` (default: `ACCESS_TOKEN`)

```csharp
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
        var issuer     = configuration["JWT_ISSUER"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_ISSUER");
        var audience   = configuration["JWT_AUDIENCE"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_AUDIENCE");
        var secret     = configuration["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_JWT_SECRET_KEY");
        var cookieName = configuration["JWT_COOKIE_NAME"] ?? "ACCESS_TOKEN";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey  = true,
                    ValidIssuer              = issuer,
                    ValidAudience            = audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(
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
```

Also add in `ConfigureServices`:
```csharp
services.AddHcLisJwtCookieAuthentication(_configuration);
```

---

## No auth variant

If the user chose "No authentication", skip this file entirely and do not add `[Authorize]`
attributes to the generated skeleton controllers.
