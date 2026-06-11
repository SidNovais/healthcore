using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.StorePatientSnapshot;

public class PatientRegisteredIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PatientRegisteredIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PatientRegisteredIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new StorePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            notification.PatientId,
            notification.FullName,
            notification.DateOfBirth,
            notification.Gender,
            notification.MothersFullName,
            notification.DocumentId,
            notification.Phone,
            notification.Email,
            notification.RegisteredAt
        )).ConfigureAwait(false);
    }
}
