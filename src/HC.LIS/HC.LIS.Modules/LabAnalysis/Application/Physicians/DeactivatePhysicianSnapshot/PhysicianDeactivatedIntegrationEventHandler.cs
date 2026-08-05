using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.DeactivatePhysicianSnapshot;

public class PhysicianDeactivatedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PhysicianDeactivatedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PhysicianDeactivatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new DeactivatePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            notification.PhysicianId,
            notification.DeactivatedAt
        )).ConfigureAwait(false);
    }
}
