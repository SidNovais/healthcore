using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.ReactivatePhysician;

internal class ReactivatePhysicianCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<ReactivatePhysicianCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        ReactivatePhysicianCommand command,
        CancellationToken cancellationToken
    )
    {
        Physician physician = await _aggregateStore.Load(new PhysicianId(command.PhysicianId)).ConfigureAwait(false) ??
            throw new InvalidCommandException("Physician must exist to reactivate");

        physician.Reactivate(command.ReactivatedAt);

        _aggregateStore.AppendChanges(physician);
    }
}
