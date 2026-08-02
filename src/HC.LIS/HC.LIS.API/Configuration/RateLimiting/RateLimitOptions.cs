namespace HC.LIS.API.Configuration.RateLimiting;

/// <summary>
/// Rate-limiting configuration, bound from the <c>RateLimit</c> section
/// (env vars: <c>ASPNETCORE_HCLIS_RateLimit__*</c>).
/// </summary>
internal sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    /// <summary>Master switch. When <c>false</c> every limiter is inert (used by tests / e2e / dev).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Baseline per-user (else per-IP) budget applied to every <c>/api/*</c> endpoint.</summary>
    public WindowLimitOptions Global { get; set; } = new() { PermitLimit = 100, WindowSeconds = 60 };

    /// <summary>Strict per-IP budget for the anonymous auth surface (login, activation).</summary>
    public WindowLimitOptions Auth { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };

    /// <summary>Max concurrent SSE stream connections per user.</summary>
    public ConcurrencyLimitOptions Stream { get; set; } = new() { PermitLimit = 5 };
}

internal sealed class WindowLimitOptions
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }

    /// <summary>Requests allowed to wait for a permit; 0 rejects immediately (the default).</summary>
    public int QueueLimit { get; set; }
}

internal sealed class ConcurrencyLimitOptions
{
    public int PermitLimit { get; set; }

    /// <summary>Connections allowed to wait for a slot; 0 rejects immediately (the default).</summary>
    public int QueueLimit { get; set; }
}
