using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class OrderItemStatus : ValueObject
{
    public string Value { get; }
    public static OrderItemStatus Requested => new("Requested");
    public static OrderItemStatus OnHold => new("OnHold");
    public static OrderItemStatus Accepted => new("Accepted");
    public static OrderItemStatus InProgress => new("InProgress");
    public static OrderItemStatus PartiallyCompleted => new("PartiallyCompleted");
    public static OrderItemStatus Completed => new("Completed");
    public static OrderItemStatus Rejected => new("Rejected");
    public static OrderItemStatus Canceled => new("Canceled");
    private OrderItemStatus(string value)
    {
        Value = value;
    }
    public static OrderItemStatus Of(string value) => new(value);
    internal bool IsRequested => Value == "Requested";
    internal bool IsOnHold => Value == "OnHold";
    internal bool IsAccepted => Value == "Accepted";
    internal bool IsInProgress => Value == "InProgress";
    internal bool IsPartiallyCompleted => Value == "PartiallyCompleted";
    internal bool IsCompleted => Value == "Completed";
    internal bool IsRejected => Value == "Rejected";
    internal bool IsCanceled => Value == "Canceled";
}
