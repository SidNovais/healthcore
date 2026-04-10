using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.Analyzer.IntegrationEvents;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ReceiveExamResult;

public class ExamResultReceivedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<ExamResultReceivedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        ExamResultReceivedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new ExamResultReceivedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.AnalyzerSampleId,
            notification.DomainEvent.WorklistItemId,
            notification.DomainEvent.ExamMnemonic,
            notification.DomainEvent.InstrumentId,
            notification.DomainEvent.ResultValue,
            notification.DomainEvent.ResultUnit,
            notification.DomainEvent.ReferenceRange,
            notification.DomainEvent.RecordedAt
        )).ConfigureAwait(false);
    }
}
