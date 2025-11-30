namespace HC.Core.Domain;

public class SystemClock
{
    private static DateTime _custom = null;
    public static DateTime Now => _custom is not null ? _custom : DateTime.UtcNow;
    public static void Set(DateTime custom) => _custom = custom;
    public static void Clear() => _custom = null;
}
