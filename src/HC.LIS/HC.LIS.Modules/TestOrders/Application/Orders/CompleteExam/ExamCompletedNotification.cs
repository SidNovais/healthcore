using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;

public class ExamCompletedNotification(OrderItemCompletedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderItemCompletedDomainEvent>(domainEvent, id)
{

}
