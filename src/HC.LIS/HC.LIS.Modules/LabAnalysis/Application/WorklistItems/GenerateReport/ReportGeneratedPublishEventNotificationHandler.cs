using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;

public class ReportGeneratedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ReportGeneratedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ReportGeneratedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new WorklistItemReportGeneratedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.WorklistItemId
        )).ConfigureAwait(false);
    }
}
