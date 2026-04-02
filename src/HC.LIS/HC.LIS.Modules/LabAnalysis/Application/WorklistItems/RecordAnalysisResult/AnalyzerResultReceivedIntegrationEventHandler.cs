using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

public class AnalyzerResultReceivedIntegrationEventNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<AnalyzerResultReceivedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        AnalyzerResultReceivedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        await _commandsScheduler.EnqueueAsync(new RecordAnalysisResultCommand(
            notification.WorklistItemId,
            notification.AnalyteCode,
            notification.ResultValue,
            notification.ResultUnit,
            notification.ReferenceRange,
            notification.InstrumentId,
            notification.RecordedAt
        )).ConfigureAwait(false);
    }
}
