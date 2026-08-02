namespace HC.LIS.API.Configuration.RateLimiting;

/// <summary>Named rate-limiting policy identifiers, attached to endpoints via <c>RequireRateLimiting</c>.</summary>
internal static class RateLimitPolicies
{
    /// <summary>Strict per-IP window for anonymous auth endpoints (login, activation).</summary>
    internal const string Auth = "auth";

    /// <summary>Per-user concurrency cap for the long-lived SSE stream.</summary>
    internal const string Stream = "stream";
}
