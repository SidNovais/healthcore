using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

public class AnalysisResultRecordedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<AnalysisResultRecordedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        AnalysisResultRecordedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new WorklistItemResultRecordedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.WorklistItemId
        )).ConfigureAwait(false);
    }
}
