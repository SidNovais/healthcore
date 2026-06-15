namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrdersList;

public class OrderListItemDto
{
    public Guid OrderId { get; set; }
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid RequestedBy { get; set; }
    public string OrderPriority { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int ItemCount { get; set; }
}
