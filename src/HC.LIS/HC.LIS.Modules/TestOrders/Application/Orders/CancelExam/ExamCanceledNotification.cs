using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;

public class ExamCanceledNotification(OrderItemCanceledDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderItemCanceledDomainEvent>(domainEvent, id)
{

}
