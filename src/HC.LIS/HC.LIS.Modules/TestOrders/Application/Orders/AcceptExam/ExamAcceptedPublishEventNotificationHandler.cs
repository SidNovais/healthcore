using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;

public class ExamAcceptedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ExamAcceptedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ExamAcceptedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new OrderItemAcceptedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderItemId,
            notification.DomainEvent.OrderId,
            notification.DomainEvent.PatientId,
            notification.DomainEvent.ContainerType
        )).ConfigureAwait(false);
    }
}
