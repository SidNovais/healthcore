using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;

public class OrderCreatedNotification(OrderCreatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderCreatedDomainEvent>(domainEvent, id)
{

}
