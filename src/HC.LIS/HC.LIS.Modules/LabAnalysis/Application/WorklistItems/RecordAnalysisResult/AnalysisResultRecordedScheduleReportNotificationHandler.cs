using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain;
using MediatR;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

internal class AnalysisResultRecordedScheduleReportNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<AnalysisResultRecordedNotification>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        AnalysisResultRecordedNotification notification,
        CancellationToken cancellationToken
    )
    {
        Guid worklistItemId = notification.DomainEvent.WorklistItemId;
        await _commandsScheduler.EnqueueAsync(new GenerateReportCommand(
            Guid.CreateVersion7(),
            worklistItemId,
            $"/reports/worklist/{worklistItemId}.pdf",
            SystemClock.Now
        )).ConfigureAwait(false);
    }
}
