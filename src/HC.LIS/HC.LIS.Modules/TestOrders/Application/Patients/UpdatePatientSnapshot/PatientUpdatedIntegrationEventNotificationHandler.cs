using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

namespace HC.LIS.Modules.TestOrders.Application.Patients.UpdatePatientSnapshot;

public class PatientUpdatedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PatientUpdatedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PatientUpdatedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        await _commandsScheduler.EnqueueAsync(new UpdatePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            notification.PatientId,
            notification.FullName,
            notification.DateOfBirth,
            notification.Gender,
            notification.MothersFullName,
            notification.DocumentId,
            notification.Phone,
            notification.Email
        )).ConfigureAwait(false);
    }
}
