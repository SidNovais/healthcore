using System;

namespace HC.Core.Domain;

public static class SystemClock
{
    private static DateTime _now;
    // Always returns UTC. Use SystemClock.Now throughout the codebase — never DateTime.Now (local-time).
    public static DateTime Now => _now == DateTime.MinValue ? DateTime.UtcNow : _now;
    public static void Set(DateTime custom) => _now = custom;
    public static void Clear() => _now = DateTime.MinValue;
}
