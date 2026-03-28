using System.Linq;
using MediatR;
using HC.Core.Domain;
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
        foreach (Guid examId in notification.ExamCodes.Select(Guid.Parse))
        {
            await _commandsScheduler.EnqueueAsync(new PlaceExamInProgressByExamIdCommand(
                Guid.CreateVersion7(),
                notification.CollectionRequestId,
                examId,
                notification.OccurredAt
            )).ConfigureAwait(false);
        }
    }
}
