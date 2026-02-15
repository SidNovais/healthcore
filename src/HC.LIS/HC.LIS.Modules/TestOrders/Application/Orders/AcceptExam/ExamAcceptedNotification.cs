using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;

public class ExamAcceptedNotification(OrderItemAcceptedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderItemAcceptedDomainEvent>(domainEvent, id)
{

}

