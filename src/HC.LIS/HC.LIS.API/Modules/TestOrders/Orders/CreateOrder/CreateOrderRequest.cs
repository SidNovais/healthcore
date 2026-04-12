namespace HC.LIS.API.Modules.TestOrders.Orders.CreateOrder;

internal sealed record CreateOrderRequest(
    Guid OrderId,
    Guid PatientId,
    Guid RequestedBy,
    string OrderPriority);
