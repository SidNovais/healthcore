using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamOnHold;

public class ExamPlacedOnHoldPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ExamPlacedOnHoldNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ExamPlacedOnHoldNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new OrderItemPlacedOnHoldIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderItemId,
            notification.DomainEvent.Reason
        )).ConfigureAwait(false);
    }
}
