using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

public class SampleCollectedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<SampleCollectedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        SampleCollectedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new SampleCollectedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.CollectionRequestId,
            notification.DomainEvent.SampleId,
            notification.DomainEvent.PatientId,
            notification.DomainEvent.SampleBarcode,
            notification.DomainEvent.ExamIds.Select(id => id.ToString()).ToList().AsReadOnly()
        )).ConfigureAwait(false);
    }
}
