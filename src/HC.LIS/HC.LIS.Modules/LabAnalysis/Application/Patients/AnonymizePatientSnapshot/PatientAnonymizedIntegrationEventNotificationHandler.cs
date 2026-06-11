using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.AnonymizePatientSnapshot;

public class PatientAnonymizedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PatientAnonymizedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PatientAnonymizedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new AnonymizePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            notification.PatientId,
            notification.AnonymizedAt
        )).ConfigureAwait(false);
    }
}
