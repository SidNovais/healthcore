using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

public class OrderDetailsDto
{
    public Guid OrderId { get; set; }
    public Guid PatientId { get; set; }
    public Guid RequestedBy { get; set; }
    public string OrderPriority { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public IReadOnlyCollection<OrderItemDetailsDto> Items { get; set; } = [];
}
