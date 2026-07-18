using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;

public class ExamRejectedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ExamRejectedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ExamRejectedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new OrderItemRejectedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderItemId,
            notification.DomainEvent.Reason
        )).ConfigureAwait(false);
    }
}
