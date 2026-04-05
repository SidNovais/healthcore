using MediatR;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

public class SampleCollectedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<SampleCollectedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        SampleCollectedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var exam in notification.Exams)
        {
            await _commandsScheduler.EnqueueAsync(new PlaceExamInProgressByExamIdCommand(
                Guid.CreateVersion7(),
                notification.CollectionRequestId,
                exam.ExamId,
                notification.OccurredAt
            )).ConfigureAwait(false);
        }
    }
}
