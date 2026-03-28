using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.HandleSampleCollected;

public class SampleCollectedIntegrationEventNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<SampleCollectedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        SampleCollectedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var examCode in notification.ExamCodes)
        {
            await _commandsScheduler.EnqueueAsync(new CreateWorklistItemCommand(
                Guid.CreateVersion7(),
                notification.SampleId,
                notification.SampleBarcode,
                examCode,
                notification.PatientId,
                notification.OccurredAt
            )).ConfigureAwait(false);
        }
    }
}
