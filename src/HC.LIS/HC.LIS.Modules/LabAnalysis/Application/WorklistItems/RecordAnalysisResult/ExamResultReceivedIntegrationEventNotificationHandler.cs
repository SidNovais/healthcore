using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.Analyzer.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

public class ExamResultReceivedIntegrationEventNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<ExamResultReceivedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        ExamResultReceivedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        await _commandsScheduler.EnqueueAsync(new RecordAnalysisResultCommand(
            notification.WorklistItemId,
            notification.ExamMnemonic,
            notification.ResultValue,
            notification.ResultUnit,
            notification.ReferenceRange,
            notification.InstrumentId,
            notification.RecordedAt
        )).ConfigureAwait(false);
    }
}
