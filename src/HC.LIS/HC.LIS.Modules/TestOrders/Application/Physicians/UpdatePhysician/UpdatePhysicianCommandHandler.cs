using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.UpdatePhysician;

internal class UpdatePhysicianCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<UpdatePhysicianCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        UpdatePhysicianCommand command,
        CancellationToken cancellationToken
    )
    {
        Physician physician = await _aggregateStore.Load(new PhysicianId(command.PhysicianId)).ConfigureAwait(false) ??
            throw new InvalidCommandException("Physician must exist to update");

        physician.Update(command.FullName, command.LicenceNumber, command.UpdatedAt);

        _aggregateStore.AppendChanges(physician);
    }
}
