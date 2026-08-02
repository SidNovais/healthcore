using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HC.LIS.API.Configuration.RateLimiting;

/// <summary>
/// Registers the API's rate-limiting policies: a global per-user (else per-IP) baseline for every
/// <c>/api/*</c> endpoint, a strict per-IP <see cref="RateLimitPolicies.Auth"/> window for the
/// anonymous auth surface, and a per-user <see cref="RateLimitPolicies.Stream"/> concurrency cap for
/// the long-lived SSE endpoint. Swagger, health probes, and the SSE stream are exempt from the global
/// window. All limiters are inert when <c>RateLimit:Enabled</c> is <c>false</c>.
/// </summary>
internal static class RateLimitingExtensions
{
    private static readonly string[] s_exemptPathPrefixes = ["/swagger", "/health"];

    internal static IServiceCollection AddHcLisRateLimiting(
        this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new RateLimitOptions();
        configuration.GetSection(RateLimitOptions.SectionName).Bind(options);

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                if (!options.Enabled || IsExemptFromGlobalLimiter(context))
                    return RateLimitPartition.GetNoLimiter("__exempt__");

                return FixedWindow(UserOrIpPartitionKey(context), options.Global);
            });

            limiter.AddPolicy(RateLimitPolicies.Auth, context =>
                !options.Enabled
                    ? RateLimitPartition.GetNoLimiter("__exempt__")
                    : FixedWindow($"auth:{ClientIp(context)}", options.Auth));

            limiter.AddPolicy(RateLimitPolicies.Stream, context =>
                !options.Enabled
                    ? RateLimitPartition.GetNoLimiter("__exempt__")
                    : Concurrency($"stream:{UserOrIpPartitionKey(context)}", options.Stream));

            limiter.OnRejected = WriteProblemDetailsAsync;
        });

        return services;
    }

    private static bool IsExemptFromGlobalLimiter(HttpContext context)
    {
        PathString path = context.Request.Path;

        // The SSE stream is long-lived; a window limiter would hold its permit forever, so it is
        // governed only by the dedicated concurrency policy.
        if (path.StartsWithSegments("/api", out PathString remainder) &&
            remainder.Value?.EndsWith("/events/stream", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        foreach (string prefix in s_exemptPathPrefixes)
        {
            if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static RateLimitPartition<string> FixedWindow(string key, WindowLimitOptions o) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = o.PermitLimit,
            Window = TimeSpan.FromSeconds(o.WindowSeconds),
            QueueLimit = o.QueueLimit,
            AutoReplenishment = true,
        });

    private static RateLimitPartition<string> Concurrency(string key, ConcurrencyLimitOptions o) =>
        RateLimitPartition.GetConcurrencyLimiter(key, _ => new ConcurrencyLimiterOptions
        {
            PermitLimit = o.PermitLimit,
            QueueLimit = o.QueueLimit,
        });

    private static string UserOrIpPartitionKey(HttpContext context)
    {
        string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId) ? $"ip:{ClientIp(context)}" : $"user:{userId}";
    }

    private static string ClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static async ValueTask WriteProblemDetailsAsync(OnRejectedContext context, CancellationToken ct)
    {
        HttpResponse response = context.HttpContext.Response;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            response.Headers.RetryAfter =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        response.StatusCode = StatusCodes.Status429TooManyRequests;
        response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Title = "Too many requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Rate limit exceeded. Please retry later.",
        };

        await response.WriteAsJsonAsync(problem, ct).ConfigureAwait(false);
    }
}
