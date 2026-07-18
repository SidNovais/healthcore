using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.Application.Patients;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;

public class OrderCreatedPublishEventNotificationHandler(
    IEventsBus eventsBus,
    IPatientSnapshotRepository patientSnapshots)
    : INotificationHandler<OrderCreatedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;
    private readonly IPatientSnapshotRepository _patientSnapshots = patientSnapshots;

    public async Task Handle(
        OrderCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        // Enrich with the patient name so the live-added list row matches a fresh load
        // (the orders-list query joins the patient snapshot for the name).
        string? patientName = await _patientSnapshots
            .GetFullNameByIdAsync(notification.DomainEvent.PatientId).ConfigureAwait(false);

        await _eventsBus.Publish(new OrderCreatedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderId,
            notification.DomainEvent.PatientId,
            notification.DomainEvent.RequestedBy,
            notification.DomainEvent.OrderPriority,
            notification.DomainEvent.RequestedAt,
            patientName
        )).ConfigureAwait(false);
    }
}
