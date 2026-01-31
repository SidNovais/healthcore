namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

public class OrderDetailsDto
{
    public Guid OrderId { get; set; }
    public Guid PatientId { get; set; }
    public Guid RequestedBy { get; set; }
    public string OrderPriority { get; set; }
    public DateTime RequestedAt { get; set; }
}
