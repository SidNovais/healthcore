using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain;
using MediatR;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.SignedReports.UploadHtmlReport;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.SignReport;

internal class SignedReportCreatedScheduleHtmlUploadNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<SignedReportCreatedNotification>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(SignedReportCreatedNotification notification, CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new UploadHtmlReportBySignedReportIdCommand(
            Guid.CreateVersion7(),
            notification.DomainEvent.ReportId,
            notification.DomainEvent.WorklistItemId,
            notification.DomainEvent.Signature,
            notification.DomainEvent.SignedBy,
            notification.DomainEvent.CreatedAt
        )).ConfigureAwait(false);
    }
}
