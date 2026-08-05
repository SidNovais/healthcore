using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.DeactivatePhysician;

internal class DeactivatePhysicianCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<DeactivatePhysicianCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        DeactivatePhysicianCommand command,
        CancellationToken cancellationToken
    )
    {
        Physician? physician = await _aggregateStore.Load(new PhysicianId(command.PhysicianId)).ConfigureAwait(false) ??
            throw new InvalidCommandException("Physician must exist to deactivate");

        physician.Deactivate(command.DeactivatedAt);

        _aggregateStore.AppendChanges(physician);
    }
}
