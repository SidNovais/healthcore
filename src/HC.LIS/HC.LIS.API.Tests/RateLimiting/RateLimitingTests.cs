using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.API.Configuration.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HC.LIS.API.Tests.RateLimiting;

/// <summary>
/// Drives the real <see cref="RateLimitingExtensions.AddHcLisRateLimiting"/> policies over an
/// in-process <see cref="TestServer"/> with a tiny middleware that injects the caller identity /
/// client IP the way authentication and Kestrel would in production.
/// </summary>
public sealed class RateLimitingTests
{
    [Fact]
    public async Task AuthPolicyReturns429WithRetryAfterWhenLimitExceeded()
    {
        await using WebApplication app = await BuildAppAsync(new Dictionary<string, string?>
        {
            ["RateLimit:Enabled"] = "true",
            ["RateLimit:Auth:PermitLimit"] = "3",
            ["RateLimit:Auth:WindowSeconds"] = "60",
            ["RateLimit:Global:PermitLimit"] = "1000",
            ["RateLimit:Global:WindowSeconds"] = "60",
        });
        using HttpClient client = app.GetTestClient();

        for (int i = 0; i < 3; i++)
        {
            HttpResponseMessage ok = await Send(client, "/auth-stub", ip: "10.0.0.1");
            ok.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        HttpResponseMessage rejected = await Send(client, "/auth-stub", ip: "10.0.0.1");

        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task AuthPolicyPartitionsPerClientIp()
    {
        await using WebApplication app = await BuildAppAsync(new Dictionary<string, string?>
        {
            ["RateLimit:Enabled"] = "true",
            ["RateLimit:Auth:PermitLimit"] = "3",
            ["RateLimit:Auth:WindowSeconds"] = "60",
            ["RateLimit:Global:PermitLimit"] = "1000",
            ["RateLimit:Global:WindowSeconds"] = "60",
        });
        using HttpClient client = app.GetTestClient();

        for (int i = 0; i < 3; i++)
            (await Send(client, "/auth-stub", ip: "10.0.0.1")).StatusCode.Should().Be(HttpStatusCode.OK);

        (await Send(client, "/auth-stub", ip: "10.0.0.1")).StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        // A different client IP has its own budget and is unaffected.
        (await Send(client, "/auth-stub", ip: "10.0.0.2")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GlobalLimiterPartitionsPerUser()
    {
        await using WebApplication app = await BuildAppAsync(new Dictionary<string, string?>
        {
            ["RateLimit:Enabled"] = "true",
            ["RateLimit:Global:PermitLimit"] = "3",
            ["RateLimit:Global:WindowSeconds"] = "60",
            ["RateLimit:Auth:PermitLimit"] = "1000",
            ["RateLimit:Auth:WindowSeconds"] = "60",
        });
        using HttpClient client = app.GetTestClient();

        for (int i = 0; i < 3; i++)
            (await Send(client, "/global-stub", userId: "user-a")).StatusCode.Should().Be(HttpStatusCode.OK);

        (await Send(client, "/global-stub", userId: "user-a")).StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        // A different authenticated user has an independent budget.
        (await Send(client, "/global-stub", userId: "user-b")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DisabledFlagBypassesAllLimiters()
    {
        await using WebApplication app = await BuildAppAsync(new Dictionary<string, string?>
        {
            ["RateLimit:Enabled"] = "false",
            ["RateLimit:Auth:PermitLimit"] = "1",
            ["RateLimit:Auth:WindowSeconds"] = "60",
            ["RateLimit:Global:PermitLimit"] = "1",
            ["RateLimit:Global:WindowSeconds"] = "60",
        });
        using HttpClient client = app.GetTestClient();

        for (int i = 0; i < 10; i++)
            (await Send(client, "/auth-stub", ip: "10.0.0.1")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExemptPathsAreNeverLimited()
    {
        await using WebApplication app = await BuildAppAsync(new Dictionary<string, string?>
        {
            ["RateLimit:Enabled"] = "true",
            ["RateLimit:Global:PermitLimit"] = "1",
            ["RateLimit:Global:WindowSeconds"] = "60",
            ["RateLimit:Auth:PermitLimit"] = "1",
            ["RateLimit:Auth:WindowSeconds"] = "60",
        });
        using HttpClient client = app.GetTestClient();

        for (int i = 0; i < 10; i++)
            (await Send(client, "/swagger/v1/swagger.json", ip: "10.0.0.1")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<HttpResponseMessage> Send(
        HttpClient client, string path, string? ip = null, string? userId = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (ip is not null)
            request.Headers.Add("X-Test-Ip", ip);
        if (userId is not null)
            request.Headers.Add("X-Test-User", userId);
        return await client.SendAsync(request);
    }

    private static async Task<WebApplication> BuildAppAsync(Dictionary<string, string?> config)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(config);

        builder.Services.AddHcLisRateLimiting(builder.Configuration);

        WebApplication app = builder.Build();

        // Stand in for authentication + Kestrel: hydrate identity / client IP from test headers
        // BEFORE the rate limiter runs so per-user / per-IP partitioning sees them.
        app.Use(async (context, next) =>
        {
            string ip = context.Request.Headers["X-Test-Ip"].ToString();
            if (!string.IsNullOrEmpty(ip))
                context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ip);

            string userId = context.Request.Headers["X-Test-User"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "test"));
            }

            await next();
        });

        app.UseRateLimiter();

        app.MapGet("/auth-stub", () => Results.Ok("ok"))
            .RequireRateLimiting(RateLimitPolicies.Auth);
        app.MapGet("/global-stub", () => Results.Ok("ok"));
        app.MapGet("/swagger/{**rest}", () => Results.Ok("ok"));

        await app.StartAsync();
        return app;
    }
}
