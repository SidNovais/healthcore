using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Orders.StoreOrderPhysician;

public class OrderCreatedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<OrderCreatedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        OrderCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new StoreOrderPhysicianByOrderIdCommand(
            Guid.CreateVersion7(),
            notification.OrderId,
            notification.RequestedBy,
            notification.RequestedAt
        )).ConfigureAwait(false);
    }
}
