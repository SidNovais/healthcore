using MediatR;
using HC.LIS.Modules.SampleCollection.Application.Collections.GenerateSampleBarcodes;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;

public class PatientWaitingNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<PatientWaitingNotification>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PatientWaitingNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _commandsScheduler.EnqueueAsync(
            new GenerateSampleBarcodesForCollectionRequestCommand(
                notification.DomainEvent.CollectionRequestId
            )
        ).ConfigureAwait(false);
    }
}
