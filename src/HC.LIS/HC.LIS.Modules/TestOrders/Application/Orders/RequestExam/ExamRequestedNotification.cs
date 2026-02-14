using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

public class ExamRequestedNotification(OrderItemRequestedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderItemRequestedDomainEvent>(domainEvent, id)
{

}
