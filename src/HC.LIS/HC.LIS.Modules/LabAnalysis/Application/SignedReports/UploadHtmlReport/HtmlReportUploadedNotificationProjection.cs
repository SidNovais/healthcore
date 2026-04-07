using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Application.Projections;
using MediatR;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.UploadHtmlReport;

public class HtmlReportUploadedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<HtmlReportUploadedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(HtmlReportUploadedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
