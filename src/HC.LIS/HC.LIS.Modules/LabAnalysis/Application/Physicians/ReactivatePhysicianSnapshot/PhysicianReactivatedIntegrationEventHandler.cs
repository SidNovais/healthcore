using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.ReactivatePhysicianSnapshot;

public class PhysicianReactivatedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PhysicianReactivatedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PhysicianReactivatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new ReactivatePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            notification.PhysicianId,
            notification.ReactivatedAt
        )).ConfigureAwait(false);
    }
}
