using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;

internal class RegisterPhysicianCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<RegisterPhysicianCommand, Guid>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task<Guid> Handle(
        RegisterPhysicianCommand command,
        CancellationToken cancellationToken
    )
    {
        Physician physician = Physician.Register(
            command.PhysicianId,
            command.FullName,
            command.LicenceNumber,
            command.RegisteredAt
        );
        _aggregateStore.Start(physician);
        return physician.Id;
    }
}
