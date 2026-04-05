using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;

public class WorklistItemCompletedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<WorklistItemCompletedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        WorklistItemCompletedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        await _commandsScheduler.EnqueueAsync(new CompleteExamByExamIdCommand(
            Guid.CreateVersion7(),
            notification.OrderId,
            notification.OrderItemId,
            notification.OccurredAt
        )).ConfigureAwait(false);
    }
}
