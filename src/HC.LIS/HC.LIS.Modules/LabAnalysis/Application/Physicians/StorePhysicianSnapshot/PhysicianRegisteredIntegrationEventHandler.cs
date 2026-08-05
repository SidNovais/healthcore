using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.StorePhysicianSnapshot;

public class PhysicianRegisteredIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PhysicianRegisteredIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PhysicianRegisteredIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new StorePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            notification.PhysicianId,
            notification.FullName,
            notification.LicenceNumber,
            notification.RegisteredAt
        )).ConfigureAwait(false);
    }
}
