using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;

public class ExamCanceledPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ExamCanceledNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ExamCanceledNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new OrderItemCanceledIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderItemId
        )).ConfigureAwait(false);
    }
}
