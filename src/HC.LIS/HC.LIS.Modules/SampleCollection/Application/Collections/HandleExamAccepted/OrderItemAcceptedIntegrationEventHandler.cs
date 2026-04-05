using MediatR;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.HandleExamAccepted;

public class OrderItemAcceptedIntegrationEventNotificationHandler(
    ICommandsScheduler commandsScheduler,
    IAggregateStore aggregateStore
) : INotificationHandler<OrderItemAcceptedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        OrderItemAcceptedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest? existing = await _aggregateStore
            .Load<CollectionRequest>(new CollectionRequestId(notification.OrderId))
            .ConfigureAwait(false);

        if (existing is null)
        {
            await _commandsScheduler.EnqueueAsync(new CreateCollectionRequestForOrderCommand(
                Guid.CreateVersion7(),
                notification.OrderId,
                notification.PatientId,
                notification.OrderItemId,
                notification.ExamMnemonic,
                notification.ContainerType,
                notification.OccurredAt
            )).ConfigureAwait(false);
        }
        else
        {
            await _commandsScheduler.EnqueueAsync(new AddExamToCollectionForOrderCommand(
                Guid.CreateVersion7(),
                notification.OrderId,
                notification.OrderItemId,
                notification.ExamMnemonic,
                notification.ContainerType
            )).ConfigureAwait(false);
        }
    }
}
