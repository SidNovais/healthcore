using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

public class ExamRequestedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ExamRequestedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ExamRequestedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new OrderItemRequestedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderItemId,
            notification.DomainEvent.OrderId,
            notification.DomainEvent.ExamMnemonic,
            notification.DomainEvent.SpecimenMnemonic,
            notification.DomainEvent.MaterialType,
            notification.DomainEvent.ContainerType,
            notification.DomainEvent.Additive,
            notification.DomainEvent.ProcessingType,
            notification.DomainEvent.StorageCondition,
            notification.DomainEvent.RequestedAt
        )).ConfigureAwait(false);
    }
}
