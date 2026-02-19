using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

public class ExamPlacedInProgressNotification(OrderItemPlacedInProgressDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<OrderItemPlacedInProgressDomainEvent>(domainEvent, id)
{

}
