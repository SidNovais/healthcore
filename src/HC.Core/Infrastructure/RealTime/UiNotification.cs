namespace HC.Core.Infrastructure.RealTime;

/// <summary>
/// A ready-to-send real-time notification for the browser. <see cref="Topic"/> selects which
/// subscribers receive it (e.g. "orders", "triage", "worklist"); <see cref="Data"/> is the
/// already-serialized JSON payload written verbatim to the SSE <c>data:</c> line.
/// </summary>
public sealed record UiNotification(string Topic, string Data);
