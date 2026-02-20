using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamOnHold;

public class ExamPlacedOnHoldNotification(OrderItemPlacedOnHoldDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderItemPlacedOnHoldDomainEvent>(domainEvent, id)
{

}
