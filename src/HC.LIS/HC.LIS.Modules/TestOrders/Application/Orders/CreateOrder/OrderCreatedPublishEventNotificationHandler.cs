using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;
using HC.LIS.Modules.TestOrders.Application.Patients;
using HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;

public class OrderCreatedPublishEventNotificationHandler(
    IEventsBus eventsBus,
    IPatientSnapshotRepository patientSnapshots,
    IQueryHandler<GetPhysicianDetailsQuery, PhysicianDetailsDto?> physicianDetails)
    : INotificationHandler<OrderCreatedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;
    private readonly IPatientSnapshotRepository _patientSnapshots = patientSnapshots;
    private readonly IQueryHandler<GetPhysicianDetailsQuery, PhysicianDetailsDto?> _physicianDetails = physicianDetails;

    public async Task Handle(
        OrderCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        string? patientName = await _patientSnapshots
            .GetFullNameByIdAsync(notification.DomainEvent.PatientId).ConfigureAwait(false);

        PhysicianDetailsDto? physician = await _physicianDetails.Handle(
            new GetPhysicianDetailsQuery(notification.DomainEvent.RequestedBy),
            cancellationToken).ConfigureAwait(false);

        await _eventsBus.Publish(new OrderCreatedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.OrderId,
            notification.DomainEvent.PatientId,
            notification.DomainEvent.RequestedBy,
            notification.DomainEvent.OrderPriority,
            notification.DomainEvent.RequestedAt,
            patientName,
            physician?.FullName
        )).ConfigureAwait(false);
    }
}
