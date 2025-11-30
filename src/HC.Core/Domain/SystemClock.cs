using System;

namespace HC.Core.Domain;

public static class SystemClock
{
    private static DateTime _now = DateTime.UtcNow;
    public static DateTime Now => _now;
    public static void Set(DateTime custom) => _now = custom;
    public static void Clear() => _now = DateTime.UtcNow;
}
