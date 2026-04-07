using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain;
using MediatR;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.SignedReports.GeneratePdf;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.UploadHtmlReport;

internal class HtmlReportUploadedSchedulePdfGenerationNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<HtmlReportUploadedNotification>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(HtmlReportUploadedNotification notification, CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new GeneratePdfBySignedReportIdCommand(
            Guid.CreateVersion7(),
            notification.DomainEvent.ReportId,
            notification.DomainEvent.WorklistItemId
        )).ConfigureAwait(false);
    }
}
