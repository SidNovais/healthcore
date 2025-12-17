using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class OrderPriority : ValueObject
{
    public string Value { get; }
    public static OrderPriority Routine => new("Routine");
    public static OrderPriority Urgent => new("Urgent");
    public static OrderPriority Stat => new("Stat");
    private OrderPriority(string value)
    {
        Value = value;
    }
    public static OrderPriority Of(string value) => new(value);
    internal bool IsRoutine => Value == "Routine";
    internal bool IsUrgent => Value == "Urgent";
    internal bool IsStat => Value == "Stat";
}
