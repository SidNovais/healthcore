using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.SignedReports.CompleteWorklistItem;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.GeneratePdf;

internal class PdfReportUploadedScheduleCompleteWorklistItemNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<PdfReportUploadedNotification>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(PdfReportUploadedNotification notification, CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new CompleteWorklistItemBySignedReportCommand(
            Guid.CreateVersion7(),
            notification.DomainEvent.WorklistItemId,
            notification.DomainEvent.UploadedAt
        )).ConfigureAwait(false);
    }
}
