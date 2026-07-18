using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PartiallyCompleteExam;

public class ExamPartiallyCompletedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ExamPartiallyCompletedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ExamPartiallyCompletedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new OrderItemPartiallyCompletedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderItemId
        )).ConfigureAwait(false);
    }
}
