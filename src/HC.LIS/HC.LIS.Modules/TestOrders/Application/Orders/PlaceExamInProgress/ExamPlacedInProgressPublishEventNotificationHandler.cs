using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

public class ExamPlacedInProgressPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ExamPlacedInProgressNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ExamPlacedInProgressNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new OrderItemPlacedInProgressIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderItemId
        )).ConfigureAwait(false);
    }
}
