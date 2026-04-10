using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;

public class WorklistItemCreatedIntegrationEventNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<WorklistItemCreatedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        WorklistItemCreatedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        await _commandsScheduler.EnqueueAsync(new AssignWorklistItemByBarcodeAndExamCodeCommand(
            Guid.NewGuid(),
            notification.SampleBarcode,
            notification.ExamCode,
            notification.WorklistItemId,
            notification.OccurredAt
        )).ConfigureAwait(false);
    }
}
