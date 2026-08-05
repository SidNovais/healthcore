using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.StorePhysicianSnapshot;

internal class StorePhysicianSnapshotByPhysicianIdCommandHandler(
    IPhysicianSnapshotRepository physicianSnapshotRepository
) : ICommandHandler<StorePhysicianSnapshotByPhysicianIdCommand>
{
    private readonly IPhysicianSnapshotRepository _physicianSnapshotRepository = physicianSnapshotRepository;

    public async Task Handle(
        StorePhysicianSnapshotByPhysicianIdCommand command,
        CancellationToken cancellationToken)
    {
        await _physicianSnapshotRepository.StoreAsync(
            command.PhysicianId,
            command.FullName,
            command.LicenceNumber,
            command.RegisteredAt
        ).ConfigureAwait(false);
    }
}
