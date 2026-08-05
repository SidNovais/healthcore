using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.UpdatePhysicianSnapshot;

public class PhysicianUpdatedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PhysicianUpdatedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PhysicianUpdatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new UpdatePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            notification.PhysicianId,
            notification.FullName,
            notification.LicenceNumber,
            notification.UpdatedAt
        )).ConfigureAwait(false);
    }
}
