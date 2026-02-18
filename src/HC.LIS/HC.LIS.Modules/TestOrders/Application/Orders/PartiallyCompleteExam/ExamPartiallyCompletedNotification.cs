using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PartiallyCompleteExam;

public class ExamPartiallyCompletedNotification(OrderItemPartiallyCompletedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderItemPartiallyCompletedDomainEvent>(domainEvent, id)
{

}
