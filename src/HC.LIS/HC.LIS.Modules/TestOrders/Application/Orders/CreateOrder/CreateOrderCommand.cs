using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;

public class CreateOrderCommand(
    Guid orderId,
    Guid patientId,
    Guid requestedBy,
    string orderPriority,
    DateTime requestedAt
) : CommandBase<Guid>
{
    public Guid OrderId { get; } = orderId;
    public Guid PatientId { get; } = patientId;
    public Guid RequestedBy { get; } = requestedBy;
    public string OrderPriority { get; } = orderPriority;
    public DateTime RequestedAt { get; } = requestedAt;
}
