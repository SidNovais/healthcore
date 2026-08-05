using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.LabAnalysis.Application.Orders;
using HC.LIS.Modules.LabAnalysis.Application.Patients;
using HC.LIS.Modules.LabAnalysis.Application.Physicians;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;

public class WorklistItemCreatedPublishEventNotificationHandler(
    IEventsBus eventsBus,
    IPatientSnapshotRepository patientSnapshots,
    IOrderPhysicianRepository orderPhysicians)
    : INotificationHandler<WorklistItemCreatedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;
    private readonly IPatientSnapshotRepository _patientSnapshots = patientSnapshots;
    private readonly IOrderPhysicianRepository _orderPhysicians = orderPhysicians;

    public async Task Handle(
        WorklistItemCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        // Enrich with the patient snapshot so the live worklist row renders identically to a
        // fresh load; null when the snapshot has not yet arrived for this patient.
        PatientSnapshotView? patient = await _patientSnapshots
            .GetByIdAsync(notification.DomainEvent.PatientId).ConfigureAwait(false);

        PhysicianSnapshotView? requestingPhysician = await _orderPhysicians
            .GetRequestingPhysicianAsync(notification.DomainEvent.OrderId).ConfigureAwait(false);

        await _eventsBus.Publish(new WorklistItemCreatedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.WorklistItemId,
            notification.DomainEvent.PatientId,
            notification.DomainEvent.SampleBarcode,
            notification.DomainEvent.ExamCode,
            patient?.FullName,
            patient?.DateOfBirth,
            patient?.Gender,
            requestingPhysician?.FullName
        )).ConfigureAwait(false);
    }
}
