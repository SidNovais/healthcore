using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.DeactivatePhysicianSnapshot;

internal class DeactivatePhysicianSnapshotByPhysicianIdCommandHandler(
    IPhysicianSnapshotRepository physicianSnapshotRepository
) : ICommandHandler<DeactivatePhysicianSnapshotByPhysicianIdCommand>
{
    private readonly IPhysicianSnapshotRepository _physicianSnapshotRepository = physicianSnapshotRepository;

    public async Task Handle(
        DeactivatePhysicianSnapshotByPhysicianIdCommand command,
        CancellationToken cancellationToken)
    {
        await _physicianSnapshotRepository.DeactivateAsync(
            command.PhysicianId,
            command.DeactivatedAt
        ).ConfigureAwait(false);
    }
}
