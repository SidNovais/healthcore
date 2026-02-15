using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;

public class ExamRejectedNotification(OrderItemRejectedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderItemRejectedDomainEvent>(domainEvent, id)
{

}

